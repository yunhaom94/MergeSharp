using Xunit;
using MergeSharp;

namespace MergeSharp.Tests;

public class PNCounterTests
{
    [Fact]
    public void TestPNCSingle()
    {
        PNCounter pnc = new PNCounter();
        pnc.Increment(5);
        pnc.Decrement(8);
        pnc.Increment(10);
        pnc.Decrement(3);

        Assert.Equal(pnc.Get(), 4);

    }

    [Fact]
    public void TestPNCMerge()
    {
        PNCounter pnc1 = new PNCounter();
        PNCounter pnc2 = new PNCounter();

        pnc1.Increment(5);
        pnc1.Decrement(8);
        pnc1.Increment(10);
        pnc1.Decrement(3);

        PNCounterMsg update = (PNCounterMsg) pnc1.GetLastSynchronizedUpdate();
        pnc2.Merge(update);

        Assert.Equal(pnc1.Get(), pnc2.Get());


    }





}

public class PNCounterMsgTests
{
    [Fact]
    public void EncodeDecode()
    {
        PNCounter pnc1 = new();
        pnc1.Increment(5);
        pnc1.Decrement(1);

        PNCounter pnc2 = new();
        pnc2.Increment(2);
        pnc1.Decrement(2);

        var encodedMsg2 = pnc2.GetLastSynchronizedUpdate().Encode();
        PNCounterMsg decodedMsg2 = new();
        decodedMsg2.Decode(encodedMsg2);

        pnc1.ApplySynchronizedUpdate(decodedMsg2);

        Assert.Equal(5-1+2-2, pnc1.Get());
    }
}
