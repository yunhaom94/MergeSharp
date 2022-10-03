using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace KVDB;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Please provide correct json cluster settings file");
            return 1;
        }

        string clusterSettings = args[0];


        Global.Init(clusterSettings);
        Server server = new Server(IPAddress.Parse(Global.cluster.selfNode.address), Global.cluster.selfNode.port);

        try
        {
            server.Start();
            Global.ksm.InitializeKeySpace();
         
            Global.logger.LogInformation("Server started.");
            Global.logger.LogInformation("Press enter to stop the server.");

            Console.ReadLine();
        }
        catch (Exception e)
        {
            Global.logger.LogInformation("Exception: {0}", e.Message);
        }
        finally
        {
            server.Stop();
            Global.logger.LogInformation("Server stopped.");
        }

        return 0;

    }
}
