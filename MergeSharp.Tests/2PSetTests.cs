using Xunit;
using MergeSharp;
using System.Collections.Generic;

namespace MergeSharp.Tests;

public class TPSetTests
{
    [Fact]
    public void TestTPsetSingle()
    {
        TPSet<string> set = new();

        set.Add("a");
        set.Add("b");
        set.Remove("a");
        Assert.False(set.Remove("c")); // this remove should have no effect
        set.Add("c");

        Assert.Equal(2, set.Count);

        Assert.Equal(set.LookupAll(), new List<string> { "b", "c" });


    }

    [Fact]
    public void TestTPsetMerge()
    {
        TPSet<string> set = new();
        set.Add("a");
        set.Add("b");

        TPSet<string> set2 = new();
        set2.Add("c");
        set2.Add("d");
        set2.Remove("c");

        set.Merge((TPSetMsg<string>)set2.GetLastSynchronizedUpdate());

        Assert.Equal(set.LookupAll(), new List<string> { "a", "b", "d" });



    }

    [Fact]
    public void TrueEquals()
    {
        TPSet<string> set = new();
        set.Add("a");
        set.Add("b");

        TPSet<string> set2 = new();
        set2.Add("a");
        set2.Add("b");

        Assert.True(set.Equals(set2));
    }

    [Fact]
    public void FalseEquals()
    {
        TPSet<string> set = new();
        set.Add("a");
        set.Add("b");

        TPSet<string> set2 = new();
        set2.Add("c");

        Assert.False(set.Equals(set2));
    }
}

public class TPSetMsgTests
{
    [Fact]
    public void EncodeDecode()
    {
        TPSet<string> set = new();
        set.Add("a");
        set.Add("b");

        TPSet<string> set2 = new();
        set2.Add("c");
        set2.Add("d");
        set2.Remove("c");

        var encodedMsg2 = set2.GetLastSynchronizedUpdate().Encode();
        TPSetMsg<string> decodedMsg2 = new();
        decodedMsg2.Decode(encodedMsg2);

        set.Merge(decodedMsg2);

        Assert.Equal(new List<string> { "a", "b", "d" }, set.LookupAll());
    }
}
