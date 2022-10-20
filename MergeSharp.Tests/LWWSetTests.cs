using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MergeSharp.Tests;

public class LWWSetTests
{
    [Fact]
    public void SingleLWWSetValueType1()
    {
        LWWSet<int> set = new();

        set.Add(1);
        set.Add(2);
        Assert.True(set.Remove(1));
        Assert.False(set.Remove(3));

        set.Add(3);

        Assert.Equal(2, set.Count);

        Assert.Equal(new List<int> { 2, 3 }, set.ToList());

        set.Clear();
        Assert.Empty(set.ToList());

        set.Add(1);
        Assert.Contains<int>(1, set);

        int[] array = new int[3];
        set.CopyTo(array, 2);

        Assert.Equal(new int[3] { 0, 0, 1 }, array);
    }

    [Fact]
    public void CopyToExceptions()
    {
        LWWSet<int> set = new();
        int[] array = new int[2];

        set.Add(1);
        set.Add(2);

        Assert.Throws<ArgumentNullException>(() => set.CopyTo(null, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.CopyTo(array, -1));
        Assert.Throws<ArgumentException>(() => set.CopyTo(array, 1));
    }

    [Fact]
    public void SingleLWWSetReferenceType()
    {
        LWWSet<string> set = new();

        set.Add("1");
        set.Add("2");
        Assert.True(set.Remove("1"));
        Assert.False(set.Remove("3"));

        set.Add("3");

        Assert.Equal(2, set.Count);

        Assert.Equal(new List<string> { "2", "3" }, set.ToList());

        set.Clear();
        Assert.Empty(set.ToList());

        set.Add("1");

        Assert.Contains<string>("1", set);
    }


    [Fact]
    public void SingleLWWSetReferenceType2()
    {
        LWWSet<string> set = new();

        set.Add("1");
        set.Add("1");

        Assert.Single(set);

        Assert.Equal(new List<string> { "1" }, set.ToList());

        set.Clear();
        set.Add("");

        Assert.Contains<string>("", set);
    }

    [Fact]
    public void Multiple()
    {
        LWWSet<int> set1 = new();
        LWWSet<int> set2 = new();

        set1.Add(1);
        set2.Add(2);

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());

        Assert.Equal(new List<int> { 1, 2 }, set1.ToList());
        Assert.Equal(new List<int> { 2 }, set2.ToList());
    }

    [Fact]
    public void ApplySynchronizedUpdateException()
    {
        LWWSet<int> set1 = new();
        LWWSet<string> set2 = new();

        set1.Add(1);
        set2.Add("2");

        Assert.Throws<NotSupportedException>(() => set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate()));
    }

    [Fact]
    public void Same()
    {
        LWWSet<int> set1 = new();
        LWWSet<int> set2 = new();
        LWWSet<int> set3 = new();

        set1.Add(1);
        set2.Add(1);
        set3.Add(2);

        Assert.Equal(set1, set2);
        Assert.False(set1 == set2);
        Assert.False(set1.Equals(set2));
        Assert.True(set1.SequenceEqual(set2));

        Assert.NotEqual(set1, set3);
        Assert.False(set1 == set3);
        Assert.False(set1.Equals(set3));
        Assert.False(set1.SequenceEqual(set3));
    }

    [Fact]
    public void HashCode()
    {
        LWWSet<int> set1 = new();
        LWWSet<int> set2 = new();
        Dictionary<LWWSet<int>, int> dict = new();

        dict[set1] = 1;
        dict[set2] = 2;

        Assert.Equal(1, dict[set1]);
    }

#nullable enable
    [Fact]
    public void AddNull()
    {
        LWWSet<string?> set1 = new();
        string? name = null;

        set1.Add(name);

        Assert.Single(set1);
        Assert.Contains(null, set1);
    }

    [Fact]
    public void RemoveNull()
    {
        LWWSet<string?> set1 = new();
        string? name = null;

        set1.Add(name);
        set1.Remove(name);

        Assert.Empty(set1.ToList());
    }

    [Fact]
    public void MergeNull()
    {
        LWWSet<string?> set1 = new();
        LWWSet<string?> set2 = new();

        set1.Add("hi");
        set1.Add(null);

        bool val = set2.Remove(null);
        Assert.False(val);

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
        Assert.Equal(new HashSet<string?> { "hi", null }, set1.ToHashSet());
    }

    [Fact]
    public void MergeNull2()
    {
        LWWSet<string?> set1 = new();
        LWWSet<string?> set2 = new();

        set1.Add("hi");
        set1.Add(null);

        set2.Add(null);
        set2.Remove(null);

        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());
        Assert.Equal(new HashSet<string?> { "hi" }, set2.ToHashSet());

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
        Assert.Equal(new HashSet<string?> { "hi" }, set1.ToHashSet());
    }
#nullable restore
}
