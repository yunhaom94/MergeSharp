using System;
using System.Text;


namespace KVDB;

public enum MsgSrc
{
    server = 1,
    client = 2,
    bftnode = 3
}

// Protocol looks like this
// \f[4 bytes: MsgSrc][4xN bytes fields for future use][4 bytes: content length][content]
public class MessagePacket
{
    // ---packet info-- 
    // Number of headerfield
    public static int NUM_FIELDS = 2;
    // 1 byte '\f' + each field is 4 bytes * N
    public static int HEADER_SIZE = 1 + NUM_FIELDS * 4;

    public MsgSrc msgSrc { set; get; }
    public int length { get; protected set; }
    public string content { get; protected set; }


    // --meta data--
    public ConnectionSession connection { get; set; }


    // create a msg from received
    public MessagePacket(MsgSrc src, int length, string content, ConnectionSession from)
    {
        this.msgSrc = src;
        this.length = length;
        this.content = content;

        this.connection = from;
    }

    // create a msg to send
    public MessagePacket(MsgSrc src, int length, string content)
    {
        this.msgSrc = src;
        this.length = length;
        this.content = content;
    }

    // can only used by subclasses
    protected MessagePacket()
    {

    }


    public byte[] Serialize()
    {
        byte[] srcb = BitConverter.GetBytes((int)this.msgSrc);
        byte[] lenb = BitConverter.GetBytes(this.length);
        byte[] contentb = Encoding.UTF8.GetBytes(this.content);

        // List<byte> msgBytes = new List<byte>();
        // msgBytes.Add((byte)'\f');
        // msgBytes.AddRange(srcb);
        // msgBytes.AddRange(lenb);
        // msgBytes.AddRange(contentb);




        byte[] data = new byte[HEADER_SIZE + contentb.Length];
        data[0] = (byte)'\f';
        Buffer.BlockCopy(srcb, 0, data, 1, srcb.Length);
        Buffer.BlockCopy(lenb, 0, data, 1 + srcb.Length, lenb.Length);
        Buffer.BlockCopy(contentb, 0, data, HEADER_SIZE, content.Length);


        //return msgBytes.ToArray();
        return data;
    }


    public override string ToString()
    {
        string msgSrcstr;
        if (this.msgSrc == MsgSrc.server)
            msgSrcstr = "server";
        else
            msgSrcstr = "client";

        return "Packet Content:\n" +
        "Sender Class: " + msgSrcstr + "\n" +
        "Length: " + this.length + "\n" +
        "Content:\n" + this.content;

    }
}

public class ClientResponse : MessagePacket
{

    public ClientResponse(bool status, string[] content) 
    {
        this.CreateResponse(status, content);
    }

    public ClientResponse(bool status, string content = "")
    {
        this.CreateResponse(status, new string[] { content });
    }

    private void CreateResponse(bool status, string[] content) 
    {
        StringBuilder str = new StringBuilder();
        if (status)
            str.AppendLine("Succeed");
        else
            str.AppendLine("Failed");

        foreach (var l in content)
        {
            str.AppendLine(l);
        }


        this.content = str.ToString();
        this.length = this.content.Length;
        this.msgSrc = MsgSrc.server;



    }

}
