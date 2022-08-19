using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using ProtoBuf;

namespace MergeSharp.TCPConnectionManager;


struct Connection
{
    public TcpClient client;
    public Task task;
}


class ManagerServer
{

    private Node self;
    // list of running connections 
    private List<Connection> Connections;
    // cancellation token to stop the server
    private CancellationTokenSource cts;

    private ConnectionManager connectionManager;

    TcpListener listener;


    public ManagerServer(Node self, ConnectionManager connectionManager)
    {
        this.Connections = new List<Connection>();
        this.self = self;
        this.connectionManager = connectionManager;

        this.Run();
    }

    public Task HandleRecieved(TcpClient client, CancellationToken ct)
    {
        NetworkStream ns = client.GetStream();

        // start a new task to handle the client
        return Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                // read the random number from the client in a loop
                NetworkProtocol syncMsg = null;
                do
                {

                    try
                    {
                        syncMsg = Serializer.DeserializeWithLengthPrefix<NetworkProtocol>(ns, PrefixStyle.Base128);
                    }
                    catch (IOException)
                    {
                        // connection closed
                        //Console.WriteLine("Connection closed");
                        break;
                    }
                    if (syncMsg != null)
                    {
                        connectionManager.ExecuteSyncMessage(syncMsg);
                    }

                    // if canceled, break out the loop
                    if (ct.IsCancellationRequested) 
                    {
                        client.Close();
                        Console.WriteLine("Client disconnected.");
                        ct.ThrowIfCancellationRequested();
                    }
                } while (syncMsg != null);



            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }, ct);

    }



    public void Run()
    {

        cts = new CancellationTokenSource();
        CancellationToken ct = cts.Token;

        this.listener = new TcpListener(this.self.ip, this.self.port);
        this.listener.Start();

        //Console.WriteLine("Manager server started.");




        // use task to accept new clients
        Task accpetClientTask = Task.Run(async () =>
        {
            ct.ThrowIfCancellationRequested();
            while (true)
            {

                var client = await listener.AcceptTcpClientAsync(ct);

                //Console.WriteLine("Client connected.");

                var t = HandleRecieved(client, ct);
                Connections.Add(new Connection() { client = client, task = t });
            }

        }, ct);


    }

    public void Stop()
    {

        // disconnet all clients from connections list
        foreach (var c in Connections)
        {
            c.client.Close();
            c.client.Dispose();
        }

        this.listener.Stop();
        cts.Cancel();
    }





}