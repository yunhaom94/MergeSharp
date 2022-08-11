using System;
using Xunit;

namespace MergeSharp.Tests;

public class SyncProtocolTests : IDisposable
{
    public SyncProtocolTests()
    {
    }

    public void Dispose()
    {
    }

    [Fact]
    public void SyncProtocolEncodeTest()
    {
        SyncProtocol sp = new SyncProtocol();
        sp.uid = Guid.NewGuid();
        sp.syncMsgType = SyncProtocol.SyncMsgType.ManagerMsg_Create;
        sp.type = "PNCounter";
        sp.message = new byte[10];
        byte[] msg = sp.Encode();
        
        SyncProtocol sp2 = SyncProtocol.Decode(msg);

        Assert.Equal(sp.uid, sp2.uid);
        Assert.Equal(sp.syncMsgType, sp2.syncMsgType);
        Assert.Equal(sp.type, sp2.type);
        Assert.Equal(sp.message, sp2.message);
    }
}
