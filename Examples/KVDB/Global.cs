using System;
using Microsoft.Extensions.Logging;

namespace KVDB;

public static class Global
{
    public static ILogger logger;
    public static ClusterManager cluster;
    public static KeySpaceManager ksm;



    public static void Init(string clusterInfoFile)
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddConsole();
        });
        logger = loggerFactory.CreateLogger("Logging");

        logger.LogInformation("Initalizing KVDB");

        cluster = new ClusterManager();
        cluster.InitClusterSettings(clusterInfoFile);
        ksm = new KeySpaceManager();
        
        
    }

}