using System.Net.Sockets;

namespace MergeSharp.TCPConnectionManager;

public class ConnectionManager : IConnectionManager
{

    private ManagerServer managerServer;
    private List<Node> nodes; 
    
    public Node selfNode { get; private set; }

    public ConnectionManager(List<Node> nodes, Node self)
    {
        this.nodes = nodes;
        this.selfNode = self;
        this.managerServer = new ManagerServer(this.selfNode, this);
    }

    // this constructor takes the ip and port for self node, and a list of strings nodes' ip and port as input
    public ConnectionManager(string ip, string port, IEnumerable<string> nodes)
    {
        this.nodes = new List<Node>();
        foreach (string n in nodes)
        {
            string[] ipAndPort = n.Split(':');
            Node node = new (ipAndPort[0], int.Parse(ipAndPort[1]));
            
            // if ip and port equal to self
            if (ipAndPort[0] == ip && ipAndPort[1] == port)
            {
                node.isSelf = true;
                this.selfNode = node;
            }
            
            this.nodes.Add(node);
        }

        this.managerServer = new ManagerServer(this.selfNode, this);
    }

    

    ~ConnectionManager()
    {
        this.Dispose();
    }

    public event EventHandler<SyncMsgEventArgs> UpdateSyncRecievedHandleEvent;


    public void ExecuteSyncMessage(SyncProtocol msg)
    {
        UpdateSyncRecievedHandleEvent(this, new SyncMsgEventArgs(msg));
    }



    public int[] AllMemberIds()
    {
        throw new NotImplementedException();
    }

    public int CurMemberPosition()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        this.Stop();
    }

    public int NumMembers()
    {
        throw new NotImplementedException();
    }

    public void PropagateSyncMsg(SyncProtocol msg)
    {
        foreach (Node node in nodes)
        {
            if (node.isSelf)
            {
                continue;
            }

            if (!node.connected)
            {
                if(!node.Connect())
                {
                    throw new Exception("Failed to connect to node " + node.ip + ":" + node.port);
                }
            }

            node.Send(msg);
        }
    }

    public void Start()
    {
        // try to connect to all clients
        foreach (Node node in nodes)
        {
            if (node.isSelf)
            {
                continue;
            }


            node.Connect();
        }
    }

    public void Stop()
    {
        this.managerServer.Stop();
        // disconnect all clients
        foreach (Node node in nodes)
        {
            if (node.isSelf)
            {
                continue;
            }
            node.Disconnect();
        }
    }
}