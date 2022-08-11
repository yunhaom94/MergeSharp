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

        set.Merge((TPSetMsg<string>)set2.GetLastSynchroizeUpdate());

        Assert.Equal(set.LookupAll(), new List<string> { "a", "b", "d" });



    }




    
}