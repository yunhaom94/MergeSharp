using System;
using Microsoft.Extensions.Logging;

namespace KVDB;

public static class Global
{
    public static ILogger logger;
    public static ClusterManager cluster;
    public static KeySpaceManager ksm;



    public static void Init(string clusterInfoFile, string loggingLevel)
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            LogLevel logLevel;
            switch (loggingLevel)
            {
                case "1":
                    logLevel = LogLevel.Debug;
                    break;
                case "2":
                    logLevel = LogLevel.Information;
                    break;
                case "3":
                    logLevel = LogLevel.Warning;
                    break;
                case "4":
                    logLevel = LogLevel.Error;
                    break;
                case "5":
                    logLevel = LogLevel.Critical;
                    break;
                case "6":
                    logLevel = LogLevel.None;
                    break;
                default:
                    logLevel = LogLevel.Debug;
                    break;
            }
            
            builder.AddConsole().SetMinimumLevel(logLevel);
            builder.AddConsole();

        });
        logger = loggerFactory.CreateLogger("Logging");

        logger.LogInformation("Initializing KVDB");

        cluster = new ClusterManager();
        cluster.InitClusterSettings(clusterInfoFile);
        ksm = new KeySpaceManager();
        
        
    }

}
