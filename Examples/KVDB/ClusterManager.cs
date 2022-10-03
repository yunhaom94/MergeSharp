
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;


namespace KVDB;

public class Node
{
    [JsonInclude]
    public int nodeid;
    [JsonInclude]
    public string address;
    [JsonInclude]
    public int port;
    public int commPort;
    [JsonInclude]
    public bool isSelf;

}

public class ClusterManager
{
    public List<Node> nodes { get; private set; }

    public Node selfNode { get; private set; }


    public ClusterManager()
    {
        nodes = new List<Node>();
    }


    public bool InitClusterSettings(string filename)
    {

        this.nodes = JsonSerializer.Deserialize<List<Node>>(File.ReadAllText(filename));

        // sanity check
        // check if multiple selves
        int selfNodeCount = 0;
        // check if duplicate nodes
        HashSet<string> addrportSet = new HashSet<string>();

        foreach (var n in nodes)
        {
            n.commPort = n.port + 3000;

            if (n.isSelf)
            {
                this.selfNode = n;
                selfNodeCount++;
            }

            if (selfNodeCount > 1)
            {
                Global.logger.LogError("Config: Too many self node!");
                return false;
            }

            // check for port validity
            if (n.port < 0 || n.port > 62535)
            {
                Global.logger.LogError("Config: Invalid port number {0}", n.port);
                return false;
            }

            string addrport = n.address + n.port.ToString();
            if (addrportSet.Contains(addrport))
                Global.logger.LogError("Duplicate nodes!");
            else
                addrportSet.Add(addrport);
        }

        if (selfNodeCount == 0)
        {
            Global.logger.LogError("Config: No self node");
            return false;
        }

        StringBuilder listingNodes = new StringBuilder("The following nodes are assigned to the cluster:\n");
        foreach (var n in nodes)
            listingNodes.AppendLine(n.nodeid + " - " + n.address + ":" + n.port.ToString());

        Global.logger.LogInformation(listingNodes.ToString());
        
        // print self node info
        Global.logger.LogInformation("Self node: " + selfNode.nodeid + " - " + selfNode.address + ":" + selfNode.port.ToString());


        return true;

    }



}
