using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using MergeSharp;
using MergeSharp.TCPConnectionManager;
using Microsoft.Extensions.Logging;

namespace KVDB;

public class KeySpaceManager : IObserver<ReceivedSyncUpdateInfo>
{
    public Dictionary<string, Guid> keySpaces;

    private ConnectionManager cm;
    private ReplicationManager rm;
    private TPSet<string> replicatedKeySet;
    private IDisposable unsubscriber;

    public KeySpaceManager()
    {
        keySpaces = new Dictionary<string, Guid>();
    }

    public void InitlizeKeySpace()
    {
        // get list of TCPConnectionManager nodes from config file
        List<MergeSharp.TCPConnectionManager.Node> tmNodes = new List<MergeSharp.TCPConnectionManager.Node>();
        MergeSharp.TCPConnectionManager.Node selfNode = null;
        foreach (var n in Global.cluster.nodes)
        {
            MergeSharp.TCPConnectionManager.Node node = new MergeSharp.TCPConnectionManager.Node(n.address, n.commPort, n.isSelf);
            tmNodes.Add(node);
            if (n.isSelf)
            {
                selfNode = node;
            }
            else
            {
                TcpClient tempConnect;
                while (true)
                {
                    try 
                    {
                        tempConnect = new TcpClient(n.address, n.port);
                        break;
                    }
                    catch (SocketException)
                    {
                        // TODO: add timeout here
                        System.Threading.Thread.Sleep(1000);
                        Global.logger.LogDebug("{0}:{1} not yet live", n.address, n.port);
                    }
                }
                
                Global.logger.LogDebug("{0}:{1} detected", n.address, n.port);
                tempConnect.Close();

            }
        }

        cm = new ConnectionManager(tmNodes, selfNode, Global.logger);
        rm = new ReplicationManager(cm);
        rm.RegisterType<TPSet<string>>();

        unsubscriber = rm.Subscribe(this);

        // get all nodeids from cluster
        List<int> nodeids = new List<int>();
        foreach (var n in Global.cluster.nodes)
        {
            nodeids.Add(n.nodeid);
        }

        // if self node is the smallest, it is responsible for creating the key space set
        if (Global.cluster.selfNode.nodeid == nodeids.Min())
        {
            // create a new key space as TPset where GUID is always 0
            var KS_uid = rm.CreateCRDTInstance<TPSet<string>>(out this.replicatedKeySet, Guid.Empty);
            Global.logger.LogDebug("Created new key space set as a 2P-Set with uid: {0}", KS_uid);
        }
        else
        {
            while(!rm.TryGetCRDT<TPSet<string>>(Guid.Empty, out this.replicatedKeySet))
            {
                // TODO: add timeout here
                System.Threading.Thread.Sleep(1000);
            }
            Global.logger.LogDebug("Got key space set from master node");
        }

        // register types
        rm.RegisterType<PNCounter>();

        Global.logger.LogInformation("Key Space Set Initialized");    
    }

    public void CreateNewKVPair<T>(string key) where T : CRDT
    {
        // add new key to key space set
        Guid guid = rm.CreateCRDTInstance<T> (out T _);

        // construct a string as "key || guid" 
        this.replicatedKeySet.Add(key + "||" + guid.ToString());
        this.keySpaces.Add(key, guid);

        Global.logger.LogDebug("Added new key {0} with GUID {1}", key, guid);
    }

    public bool GetKVPair<T>(string key, out T value) where T : CRDT
    {
        Guid guid = this.keySpaces[key];
        if (rm.TryGetCRDT<T>(guid, out value))
            return true;
        else
            return false;
    }

    private List<string> LastKeySpaceSet = new List<string>(); 
    public void OnNext(ReceivedSyncUpdateInfo value)
    {
        // TODO: find a more optmized way to do this
        // if guid is 0, then this is the key space set
        Console.WriteLine("1");
        if (value.guid == Guid.Empty)
        {
            Console.WriteLine("2");
            var newSet = replicatedKeySet.LookupAll();
            // compare the differnce between new and old keyspace set
            var diff = newSet.Except(LastKeySpaceSet);
            foreach (var item in diff)
            {
                Console.WriteLine("3");
                // if the key is not in the key space set, then it is a new key
                if (!this.keySpaces.ContainsKey(item.Split("||")[0]))
                {
                    // add new key to key space set
                    this.keySpaces.Add(item.Split("||")[0], Guid.Parse(item.Split("||")[1]));
                    Global.logger.LogDebug("New key {0} with GUID {1} sync'd", item.Split("||")[0], Guid.Parse(item.Split("||")[1]));
                }
            }

            LastKeySpaceSet = newSet;
        }
    }

    public void OnCompleted()
    {
        unsubscriber.Dispose();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }


}
