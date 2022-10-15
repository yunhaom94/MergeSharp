

using System;
using System.Collections.Generic;
using System.IO;
using MergeSharp;
using Microsoft.Extensions.Logging;

namespace KVDB;

public class Command
{
    public readonly string key;
    public readonly string typeCode;
    public readonly string opCode;


    public readonly List<string> paramsList;
    public readonly int size;


    public Command(string key, string typeCode, string opCode, List<string> paramsList)
    {
        this.key = key;
        this.typeCode = typeCode;
        this.opCode = opCode;
        this.paramsList = paramsList;
        this.size = paramsList.Count;


    }


    public static Command ParseCommand(MessagePacket msg)
    {
        string typeCode = "";
        string key = "";
        string apiCode = "";

        List<string> parameters = new List<string>();

        var cmd = msg.content.Trim('\0');
        var source = msg.msgSrc;

        using (StringReader reader = new StringReader(cmd))
        {
            string line;
            bool onParam = false;
            string paramstr = "";

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length > 1 && line[1] == '&')
                {

                    string content = line.Remove(0, 2).Trim('\n', ' ');

                    if (line[0] == 't')
                        typeCode = content;
                    else if (line[0] == 'u')
                        key = content;
                    else if (line[0] == 'o')
                        apiCode = content;
                    else if (line[0] == 'c')
                    {
                        continue;
                    }
                    else if (line[0] == 'p')
                    {
                        // if not the first P: seen
                        if (onParam)
                            parameters.Add(paramstr);

                        // first line of param str
                        paramstr = content;
                        onParam = true;

                    }

                }
                else if (onParam)
                {
                    // in paramstring block
                    paramstr += line;
                }
                else
                {
                    throw new Exception("Unknown line parsed " + line);
                }
            }
            // last param
            if (onParam)
                parameters.Add(paramstr);
        }

        // return the command
        return new Command(key, typeCode, apiCode, parameters);
    }

    public override string ToString()
    {
        return "Command: " + this.key + " " + this.typeCode + " " + this.opCode + " " + this.paramsList.ToArray().ToString();
    }

    public ClientResponse Execute(KeySpaceManager ksm)
    {
        // pnc
        string key = this.key;

        try
        {
            if (typeCode == "pnc")
            {
                if (opCode == "s")
                {
                    ksm.CreateNewKVPair<PNCounter>(key);
                    return new ClientResponse(true); 
                }
                else
                {
                    if (ksm.GetKVPair<PNCounter>(key, out PNCounter instance))
                    {
                        if (opCode == "i")
                        {
                            instance.Increment(int.Parse(paramsList[0]));
                            Global.logger.LogDebug("PNCounter incremented by {0}", paramsList[0]);
                            return new ClientResponse(true);
                        }
                        else if (opCode == "d")
                        {
                            instance.Decrement(int.Parse(paramsList[0]));
                            Global.logger.LogDebug("PNCounter decremented by {0}", paramsList[0]);
                            return new ClientResponse(true);
                        }
                        else if (opCode == "g")
                        {
                            return new ClientResponse(true, instance.Get().ToString());
                        }
                    }
                    else
                    {
                        return new ClientResponse(false, "Key not found");
                    }
                }
            }
        }
        catch (Exception e)
        {

            Global.logger.LogError("Error in handling commands: {0} at {1}", e.Message, e.StackTrace);
            return new ClientResponse(false, e.Message);
        }

        Global.logger.LogWarning("Unknown command: " + this.ToString());
        return new ClientResponse(false, "Unknown command");

    }

}

