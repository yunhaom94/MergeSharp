using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using NetCoreServer;

namespace KVDB;


public class ConnectionSession : TcpSession
{
    private int state = 0; // 0 = searching for 'f', 1 = completeting header, 2 = completetting content 
    private int headerReadCount, contentReadCount;
    private byte[] headerRead = new byte[MessagePacket.HEADER_SIZE - 1];
    private byte[] contentRead;

    public ConnectionSession(TcpServer server) : base(server)
    {
    }

    protected override void OnConnecting()
    {
        var client = IPAddress.Parse(((IPEndPoint)this.Socket.RemoteEndPoint).Address.ToString()) + ":" + ((IPEndPoint)this.Socket.RemoteEndPoint).Port.ToString();
        Console.WriteLine("New client from " + client + " connected");
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {

        int leftToRead;
        for (long i = offset; i < offset + size; i++)
        {
            if (state == 0)
            {
                if (buffer[i] == '\f')
                    state = 1;
            }
            else if (state == 1)
            {
                leftToRead = MessagePacket.HEADER_SIZE - 1 - headerReadCount;

                if (i + leftToRead > size)
                {
                    System.Buffer.BlockCopy(buffer, (int)i, headerRead, headerReadCount, (int)(size - i));
                    headerReadCount += (int)(size - i);
                }
                else
                {
                    // finished reading header

                    System.Buffer.BlockCopy(buffer, (int)i, headerRead, headerReadCount, leftToRead);
                    headerReadCount += leftToRead;

                    state = 2;
                }

                i += leftToRead - 1;
            }
            else if (state == 2)
            {

                MsgSrc src = (MsgSrc)BitConverter.ToInt32(headerRead);
                int contentlen = BitConverter.ToInt32(headerRead, (MessagePacket.NUM_FIELDS - 1) * 4);

                if (contentReadCount == 0)
                {
                    // init buffer
                    contentRead = ArrayPool<byte>.Shared.Rent(contentlen);
                }

                leftToRead = contentlen - contentReadCount;

                if (i + leftToRead > size)
                {
                    System.Buffer.BlockCopy(buffer, (int)i, contentRead, contentReadCount, (int)(size - i));

                    contentReadCount += (int)(size - i);
                }
                else
                {
                    // finished reading content

                    System.Buffer.BlockCopy(buffer, (int)i, contentRead, contentReadCount, leftToRead);

                    contentReadCount += leftToRead;

                    MessagePacket msg = new MessagePacket(src, contentlen, Encoding.UTF8.GetString(contentRead), this);
                    Console.WriteLine("Recieveing msg:\n " + msg);
                    //this.reqQueue.Writer.WriteAsync(msg);


                    this.HandleRequest(msg);

                    state = 0;
                    headerReadCount = 0;
                    contentReadCount = 0;
                }
                i += leftToRead - 1;
            }
        }
    }

    private void HandleRequest(MessagePacket msg)
    {
        Console.WriteLine("Handling msg:\n " + msg);

        var cmd = Command.ParseCommand(msg);


    }
}

public class Server : TcpServer
{
    public Server(IPAddress address, int port) : base(address, port)
    {
    }

    protected override TcpSession CreateSession()
    {
        return new ConnectionSession(this);
    }

}