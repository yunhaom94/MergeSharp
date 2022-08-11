

using System;
using System.Collections.Generic;
using System.IO;

namespace KVDB;

public class Command
{
    public readonly string uid;
    public readonly string typeCode;
    public readonly string opCode;


    public readonly List<string> paramsList;
    public readonly int size;


    public Command(string uid, string typeCode, string opCode, List<string> paramsList)
    {
        this.uid = uid;
        this.typeCode = typeCode;
        this.opCode = opCode;
        this.paramsList = paramsList;
        this.size = paramsList.Count;


    }


    public static Command ParseCommand(MessagePacket msg)
    {
        string typeCode = "";
        string uid = "";
        string apiCode = "";

        List<string> parameters = new List<string>();

        var cmd = msg.content;
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
                        uid = content;
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
                    throw new Exception("Unkown line parsed " + line);
                }
            }
            // last param
            if (onParam)
                parameters.Add(paramstr);
        }

        // return the command
        return new Command(uid, typeCode, apiCode, parameters);
    }

}