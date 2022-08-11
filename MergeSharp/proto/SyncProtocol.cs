using System;
using System.IO;
using ProtoBuf;

namespace MergeSharp
{   
    [ProtoContract]
    public class SyncProtocol
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

        public SyncProtocol()
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
                Serializer.Serialize<SyncProtocol>(memoryStream, this);
                return memoryStream.ToArray();
            }

        }

        /// <summary>
        /// Decode a json serialzation of a replication message
        /// </summary>
        /// <param name="msg"></param>
        public static SyncProtocol Decode(byte[] msg)
        {
            using(var memoryStream = new MemoryStream(msg))
            {
                return Serializer.Deserialize<SyncProtocol>(memoryStream);
            }
        }

    }


}
