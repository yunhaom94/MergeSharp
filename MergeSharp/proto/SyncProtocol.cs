using System;
using System.IO;
using ProtoBuf;

namespace MergeSharp
{   
    /// <summary>
    /// A message protocol used to propagate updates to other nodes/replicas.
    /// This class uses protobuf to serialize and deserialize messages.
    /// </summary>
    [ProtoContract]
    public class NetworkProtocol
    {

        public enum SyncMsgType
        {
            ManagerMsg_Create,
            CRDTMsg
        }


        [ProtoMember(1)]
        public Guid uid;
        [ProtoMember(2)]
        public SyncMsgType syncMsgType;
        [ProtoMember(3)]
        public string type;
        [ProtoMember(4)]
        public byte[] message;

        public NetworkProtocol()
        {
        }

        /// <summary>
        /// A simple json serialzation of the replication message
        /// </summary>
        /// <returns></returns>
        public byte[] Encode()
        {
            using(var memoryStream = new MemoryStream())
            {
                Serializer.Serialize<NetworkProtocol>(memoryStream, this);
                return memoryStream.ToArray();
            }

        }

        /// <summary>
        /// Decode a json serialzation of a replication message
        /// </summary>
        /// <param name="msg"></param>
        public static NetworkProtocol Decode(byte[] msg)
        {
            using(var memoryStream = new MemoryStream(msg))
            {
                return Serializer.Deserialize<NetworkProtocol>(memoryStream);
            }
        }

    }


}
