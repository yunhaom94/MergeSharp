using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace KVDB;

class Program
{
    static int Main(string[] args)
    {
        string helpStr = @"Usage: KVDB <cluster_config_file> <logging_level>
Cluster Config File: See cluster_config_sample.json
Logging Level: Debug = 1, Information = 2, Warning = 3, Error = 4, Critical = 5, None = 6; default = 1";
//Log File: If not specified, log will be written to console."; TODO

        string clusterConfig;
        string debugLevel = "1";
        string logFile = "";

        if (args.Length == 1)
        {
            clusterConfig = args[0];
            return 1;
        }
        else if (args.Length == 2)
        {
            clusterConfig = args[0];
            debugLevel = args[1];
        }
        else if (args.Length == 3)
        {
            clusterConfig = args[0];
            debugLevel = args[1];
            logFile = args[2];
        }
        else
        {
            Console.WriteLine(helpStr);
            return 1;
        }

        Global.Init(clusterConfig, debugLevel);
        Server server = new Server(IPAddress.Parse("0.0.0.0"), Global.cluster.selfNode.port);

        
        Global.logger.LogDebug("test debug");
        Global.logger.LogInformation("test info");
        Global.logger.LogWarning("test warning");
        Global.logger.LogError("test error");
        Global.logger.LogCritical("test critical");

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
