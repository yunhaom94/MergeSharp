using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MergeSharp
{

    /// <summary>
    /// This is the class used by the Observer/Observable for received new updates.
    /// RM will notify any observers on new updates to a CRDT object.
    /// </summary>
    public class ReceivedSyncUpdateInfo
    {
        public readonly Guid guid;

        public ReceivedSyncUpdateInfo(Guid guid)
        {
            this.guid = guid;
        }
    }


    /// <summary>
    /// This is the main class handles CRDT replication. Each instance of a ReplicationManager
    /// can be seen as a replica (if they are connected via ConnectionManager).
    /// See README for more details.
    /// </summary>
    public class ReplicationManager : IDisposable, IObservable<ReceivedSyncUpdateInfo>
    {
        // ===============Connection managers================
        public static IConnectionManager globalConnectionManager = null;
        private IConnectionManager connectionManager;
        // ===============Type and instances helpers================
        private ProxyBuilder proxyBuilder;
        // TODO: maybe add secondary table so each type can have a different lookup table
        // or maybe dedicated storage
        // second_table = { guid: primary_table} primary_table = { guid: object }
        private IDictionary<Guid, CRDT> objectLookupTable;
        private Dictionary<string, Type> registeredTypes;
        // ===============Other helpers================
        private readonly ILogger logger;
        private List<IObserver<ReceivedSyncUpdateInfo>> observers;

        public ReplicationManager(IConnectionManager connectionManager = null, IDictionary<Guid, CRDT> objectStorage = null, ILogger logger = null)
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

            this.logger = logger ?? NullLogger.Instance;
            this.observers = new List<IObserver<ReceivedSyncUpdateInfo>>();

            this.connectionManager.ReplicationManagerSyncMsgHandlerEvent += HandleReceivedSyncUpdate;
            this.connectionManager.Start();

        }

        ~ReplicationManager()
        {
            this.Dispose();
        }

        
        public void Dispose()
        {
            this.connectionManager.Stop();
            this.connectionManager.Dispose();
            foreach (var ob in this.observers)
            {
                ob.OnCompleted();
            }
        }

        /// <summary>
        /// Retrieve a CRDT instance by the given Guid.
        /// </summary>
        /// <param name="id">Instance Guid</param>
        /// <param name="crdtInstance">Out to CRDT Instance, null if instance not found</param>
        /// <typeparam name="T">Instance type</typeparam>
        /// <returns> Whether the instance is found or not </returns>
        public bool TryGetCRDT<T>(Guid id, out T crdtInstance) where T : CRDT
        {
            CRDT instance;
            var result = this.objectLookupTable.TryGetValue(id, out instance);
            if (result)
                crdtInstance = (T)instance;
            else
                crdtInstance = null;

            return result;
        }

        /// <summary>
        /// Retrieve a CRDT instance by given Guid. Throws KeyNotFoundException if instance not found.
        /// </summary>
        /// <param name="id">Instance Guid</param>
        /// <typeparam name="T">Instance type</typeparam>
        /// <returns>CRDT Instance</returns>
        public T GetCRDT<T>(Guid id) where T : CRDT
        {
            T instance;
            if (this.TryGetCRDT<T>(id, out instance))
                return instance;
            else
                throw new KeyNotFoundException($"No CRDT with id {id} found!");

        }

        /// <summary>
        /// Get all Guids.
        /// </summary>
        /// <returns>A list of Guids for all stored CRDT instances</returns>
        public List<Guid> GetAlluids()
        {
            return new List<Guid>(this.objectLookupTable.Keys);
        }

        /// <summary>
        /// Get the type of a CRDT instance.
        /// </summary>
        /// <param name="id">Instance Guid </param>
        /// <returns></returns>
        public Type GetCRDTType(Guid id)
        {
            return this.objectLookupTable[id].GetType();
        }

        /// <summary>
        /// Store a CRDT instance.
        /// </summary>
        /// <param name="crdtObject"> The CRDT instance to store </param>
        /// <param name="uid"> The Guid of this instance </param>
        /// <typeparam name="T">Instance type</typeparam>
        /// <returns>The Guid of this instance</returns>
        private Guid Record<T>(T crdtObject, Guid uid) where T : CRDT
        {
            this.objectLookupTable[uid] = crdtObject;
            crdtObject.manager = this;
            crdtObject.uid = uid;

            // sync this new object
            NetworkProtocol syncMsg = new NetworkProtocol();
            syncMsg.uid = crdtObject.uid;
            syncMsg.syncMsgType = NetworkProtocol.SyncMsgType.ManagerMsg_Create;
            syncMsg.type = typeof(T).ToString();
            this.connectionManager.PropagateSyncMsg(syncMsg);

            return uid;
        }

        /// <summary>
        /// Register all types in the assembly under namespace "MergeSharp"
        /// // TODO: make a namespace just for types
        /// </summary>
        public void AutoTypeRegistration()
        {
            
        }

        /// <summary>
        /// Register a CRDT type. Any CRDT type must be registered before it can be replicated using ReplicationManager.
        /// It uses proxyBuilder to create a wrapped proxy classes for CRDT update methods, so that updates 
        /// can be synchronized to other replicas/ReplicationManager.
        /// </summary>
        /// <typeparam name="T">CRDT type to register</typeparam>
        public void RegisterType<T>() where T : CRDT
        {

            var t = typeof(T);

            // if type is already registered, throw exception
            if (this.registeredTypes.ContainsKey(t.ToString()))
                throw new ArgumentException("Given type to register " + t.ToString() + " is already registered!");

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
                throw new ArgumentException("Given type to register " + t.ToString() + " does not have a corresponding Anti-Entropy protocol!");
            else if (count> 1)
                throw new ArgumentException("Given type to register " + t.ToString() + " has more than one corresponding Anti-Entropy protocol!");


            this.registeredTypes[t.ToString()] = proxyBuilder.BuildProxiedClass(t);

            logger.LogInformation("Registered new type " + t.ToString());


        }


        /// <summary>
        /// Create a new CRDT instance. This method must be used to 
        /// create new CRDT instances that can be replicated with the ReplicationManager.
        /// </summary>
        /// <param name="instance">Out to CRDT instance</param>
        /// <typeparam name="T">Instance type</typeparam>
        /// <returns>Guid of the instance</returns>
        public Guid CreateCRDTInstance<T>(out T instance) where T : CRDT
        {
            return CreateCRDTInstance<T>(out instance, Guid.NewGuid());
        }

        /// <summary>
        /// Create a new CRDT instance with a pre-defined Guid. This method must be used to 
        /// create new CRDT instances that can be replicated with the ReplicationManager.
        /// </summary>
        /// <param name="instance">Out to CRDT instance</param>
        /// <param name="uid">Guid of the instance</param>
        /// <typeparam name="T">Instance type</typeparam>
        /// <returns>Guid of the instance</returns>
        public Guid CreateCRDTInstance<T>(out T instance, Guid uid) where T : CRDT
        {

            if (!this.registeredTypes.ContainsKey(typeof(T).ToString()))
                throw new ArgumentException("Given type to create " + typeof(T).ToString() + " is not registered!");

            Type t = this.registeredTypes[typeof(T).ToString()];
            instance = (T)Activator.CreateInstance(t);

            return this.Record<T>(instance, uid);
        }


        private void HandleReceivedSyncUpdate(object sender, SyncMsgEventArgs e)
        {

            NetworkProtocol msg = e.msg;

            switch (msg.syncMsgType)
            {
                case NetworkProtocol.SyncMsgType.CRDTMsg:
                    this.ReceivedUpdateSyncMsg(msg.uid, msg.message);
                    break;
                case NetworkProtocol.SyncMsgType.ManagerMsg_Create:
                    this.ReceivedNewObjectSyncMsg(msg.uid, msg.type);
                    break;
            }

        }



        // TODO: ReplicatedDataTypes change this
        private void ReceivedNewObjectSyncMsg(Guid uid, string crdtType)
        {

            if (!this.registeredTypes.ContainsKey(crdtType))
                throw new ArgumentException("Given type to create " + crdtType+ " is not registered!");


            var type = this.registeredTypes[crdtType];
            var crdtObject = (CRDT)Activator.CreateInstance(type);
            crdtObject.manager = this;
            crdtObject.uid = uid;
            
            this.objectLookupTable[uid] = crdtObject;

        }

        // Apply a synchronized update to a CRDT object, this method is thread safe
        private void ReceivedUpdateSyncMsg(Guid uid, byte[] msg)
        {
            CRDT crdtObject = this.objectLookupTable[uid];

            var propmsg = crdtObject.DecodePropagationMessage(msg);

            lock (crdtObject)
            {
                crdtObject.ApplySynchronizedUpdate(propmsg);
            }

            // notify observers that there is an update to this object
            foreach (var ob in this.observers)
            {
                ob.OnNext(new ReceivedSyncUpdateInfo(uid));
            }

        }


        internal void NewUpdateToSync(CRDT crdtObject)
        {
            NetworkProtocol syncMsg = new NetworkProtocol();
            syncMsg.uid = crdtObject.uid;
            syncMsg.syncMsgType = NetworkProtocol.SyncMsgType.CRDTMsg;
            lock (crdtObject)
            {
                syncMsg.message = crdtObject.GetLastSynchronizedUpdate().Encode();
            }
            this.connectionManager.PropagateSyncMsg(syncMsg);
        }


        private class Unsubscriber : IDisposable
        {
            private List<IObserver<ReceivedSyncUpdateInfo>> _observers;
            private IObserver<ReceivedSyncUpdateInfo> _observer;

            public Unsubscriber(List<IObserver<ReceivedSyncUpdateInfo>> observers, IObserver<ReceivedSyncUpdateInfo> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }


        /// <summary>
        /// Subscribe to get notified when there is an update to a CRDT object.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IObserver<ReceivedSyncUpdateInfo> observer)
        {
            this.observers.Add(observer);
            return new Unsubscriber(this.observers, observer);
        }
    }


}


