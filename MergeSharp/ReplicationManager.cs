using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace MergeSharp
{


    public class ReplicationManager : IDisposable
    {
        public static IConnectionManager globalConnectionManager = null;

        // TODO: maybe add secondary table so each type can have a different lookup table
        // or maybe dedicated storage
        // second_table = { guid: pimary_table} primary_table = { guid: object }
        private IDictionary<Guid, CRDT> objectLookupTable;

        private IConnectionManager connectionManager;


        private Dictionary<string, Type> registeredTypes;

        private ProxyBuilder proxyBuilder;

        public ReplicationManager(IConnectionManager connectionManager = null, IDictionary<Guid, CRDT> objectStorage = null)
        {

            this.registeredTypes = new Dictionary<string, Type>();
            this.proxyBuilder = new ProxyBuilder();

            if (objectStorage == null)
            {
                this.objectLookupTable = new ConcurrentDictionary<Guid, CRDT>();
            }

            if (connectionManager is not null)
                this.connectionManager = connectionManager;
            else
            {
                if (globalConnectionManager is null)
                    throw new ArgumentNullException(nameof(globalConnectionManager), "GlobalConnectionManager is null and no connectionManager is specified!");

                this.connectionManager = globalConnectionManager;
            }

            this.connectionManager.UpdateSyncRecievedHandleEvent += HandleRecievedSyncUpdate;
            this.connectionManager.Start();

        }

        ~ReplicationManager()
        {
            this.Dispose();
        }



        public T GetCRDT<T>(Guid id) where T : CRDT
        {
            CRDT crdt;
            if (this.objectLookupTable.TryGetValue(id, out crdt))
                return (T)crdt;

            // foreach (var item in this.GetAlluids())
            // {   
            //     Console.WriteLine(item);
            // } 

            throw new MissingFieldException("CRDT Object with uid " + id.ToString() + " is not registered for the manager!");
        }

        public List<Guid> GetAlluids()
        {
            return new List<Guid>(this.objectLookupTable.Keys);
        }


        private Guid Record<T>(T crdtObject, Guid uid) where T : CRDT
        {
            this.objectLookupTable[uid] = crdtObject;
            crdtObject.manager = this;
            crdtObject.uid = uid;

            // sync this new object
            SyncProtocol syncMsg = new SyncProtocol();
            syncMsg.uid = crdtObject.uid;
            syncMsg.syncMsgType = SyncProtocol.SyncMsgType.ManagerMsg_Create;
            syncMsg.type = typeof(T).Name;
            this.connectionManager.PropagateSyncMsg(syncMsg);

            return uid;
        }

        /// <summary>
        /// Regsiter all types in the assembly under namespace "MergeSharp"
        /// // TODO: make a namespace just for types
        /// </summary>
        public void AutoTypeResgistration()
        {
            
        }


        public void RegisterType<T>() where T : CRDT
        {

            var t = typeof(T);

            // if type is already registered, throw exception
            if (this.registeredTypes.ContainsKey(t.ToString()))
                throw new ArgumentException("Given type to register " + t.Name + " is already registered!");

            // look for the class in the assembly with TypeAntiEntropyProtocol attribute
            var aeps = Assembly.GetExecutingAssembly().GetTypes().Where(c => c.IsDefined(typeof(TypeAntiEntropyProtocolAttribute)));

            int count = 0;
            foreach (var proto in aeps)
            {
                // get the attribute from the class
                var attr = proto.GetCustomAttribute<TypeAntiEntropyProtocolAttribute>();
                if (attr.type.Name == t.Name)
                {
                    count++;
                }

            }

            // if no type is found, throw exception
            if (count == 0)
                throw new ArgumentException("Given type to register " + t.Name + " does not have a corresponding Anti-Entropy protocol!");
            else if (count> 1)
                throw new ArgumentException("Given type to register " + t.Name + " has more than one corresponding Anti-Entropy protocol!");



            this.registeredTypes[t.ToString()] = proxyBuilder.BuildProxiedClass(t);



        }



        public Guid CreateCRDTInstance<T>(out T instance) where T : CRDT
        {
            return CreateCRDTInstance<T>(out instance, Guid.NewGuid());
        }

        public Guid CreateCRDTInstance<T>(out T instance, Guid uid) where T : CRDT
        {

            if (!this.registeredTypes.ContainsKey(typeof(T).ToString()))
                throw new ArgumentException("Given type to create " + typeof(T).Name + " is not registered!");

            Type t = this.registeredTypes[typeof(T).ToString()];
            T crdtObject = (T)Activator.CreateInstance(t);
            instance = crdtObject;
            return this.Record<T>(crdtObject, uid);
        }



        // public int GetNumReplicas()
        // {
        //     return this.connectionManager.NumMembers();
        // }

        // public int GetReplicaIdx()
        // {
        //     return this.connectionManager.CurMemberPosition();
        // }




        public void HandleRecievedSyncUpdate(object sender, SyncMsgEventArgs e)
        {

            SyncProtocol msg = e.msg;

            switch (msg.syncMsgType)
            {
                case SyncProtocol.SyncMsgType.CRDTMsg:
                    this.RecievedUpdateSyncMsg(msg.uid, msg.message);
                    break;
                case SyncProtocol.SyncMsgType.ManagerMsg_Create:
                    this.RecievedNewObjectSyncMsg(msg.uid, msg.type);
                    break;
            }

        }



        // TODO: ReplicatedDataTypes change this
        private void RecievedNewObjectSyncMsg(Guid uid, string crdtType)
        {


            // CRDT crdtObject = CRDTFactory.GetFactory(crdtType).Create();
            // crdtObject.manager = this;
            // crdtObject.uid = uid;

            if (!this.registeredTypes.ContainsKey(crdtType))
                throw new ArgumentException("Given type to create " + crdtType+ " is not registered!");


            var type = this.registeredTypes[crdtType.ToString()];
            var crdtObject = (CRDT)Activator.CreateInstance(type);
            crdtObject.manager = this;
            crdtObject.uid = uid;
            
            this.objectLookupTable[uid] = crdtObject;

        }

        // Apply a synchroized update to a CRDT object, this method is thread safe
        private void RecievedUpdateSyncMsg(Guid uid, byte[] msg)
        {
            CRDT crdtObject = this.objectLookupTable[uid];

            var propmsg = crdtObject.DecodePropagationMessage(msg);

            lock (crdtObject)
            {
                crdtObject.ApplySynchronizedUpdate(propmsg);
            }

        }



        internal void NewUpdateToSync(CRDT crdtObject)
        {
            SyncProtocol syncMsg = new SyncProtocol();
            syncMsg.uid = crdtObject.uid;
            syncMsg.syncMsgType = SyncProtocol.SyncMsgType.CRDTMsg;
            lock (crdtObject)
            {
                syncMsg.message = crdtObject.GetLastSynchroizeUpdate().Encode();
            }
            this.connectionManager.PropagateSyncMsg(syncMsg);
        }

        public void Dispose()
        {
            this.connectionManager.Stop();
            this.connectionManager.Dispose();
        }
    }



}


