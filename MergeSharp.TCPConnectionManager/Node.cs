using System.Net;
using System.Net.Sockets;
using ProtoBuf;

namespace MergeSharp.TCPConnectionManager;

/// <summary>
/// This is the class that represents a node in the cluster
/// </summary>
public class Node
{
    public int id { get; private set; }
    
    // ip address
    public IPAddress ip { get; private set; }

    // port
    public int port { get; private set; }

    // tcp sessoion
    public TcpClient tcpSession { get; private set; }

    public Stream stream { get; private set; } = null;

    // is self
    public bool isSelf;

    // connected
    public bool connected { get; private set; }

    // constructor that takes ip as a string
    public Node(string ip, int port, bool isSelf=false)
    {
        this.ip = IPAddress.Parse(ip);
        this.port = port;
        this.isSelf = isSelf;
        this.connected = false;
        this.tcpSession = new TcpClient();
    }


    public bool Connect()
    {
        try
        {
            this.tcpSession.Connect(this.ip, this.port);
            this.connected = true;
            return true;
        }
        catch (SocketException)
        {
            this.connected = false;
            return false;
        }

    }

    public void Send(SyncProtocol msg)
    {
       // if stream is null get stream
        if (this.stream == null)
        {
            this.stream = this.tcpSession.GetStream();
        }

        Serializer.SerializeWithLengthPrefix(this.stream, msg, PrefixStyle.Base128);
        
    }

    public void Send(byte[] msg)
    {
        if (this.stream == null)
        {
            this.stream = this.tcpSession.GetStream();
        }

        stream.Write(msg, 0, msg.Length);

    }

    public void Disconnect()
    {
        this.tcpSession.Close();
    }
    


}



