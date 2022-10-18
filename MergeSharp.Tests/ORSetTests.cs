using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MergeSharp.Tests;

public class ORSetTests
{
    [Fact]
    public void SingleORSetValueType1()
    {
        ORSet<int> set = new();

        set.Add(1);
        set.Add(2);
        Assert.True(set.Remove(1));
        Assert.False(set.Remove(3));

        set.Add(3);

        Assert.Equal(2, set.Count);

        Assert.Equal(new List<int> { 2, 3 }, set.LookupAll().OrderBy(t => t));

        set.Clear();
        Assert.Equal(0, set.Count);
        Assert.Equal(new List<int> { }, set.LookupAll().OrderBy(t => t));

        Assert.False(set.Contains(1));

        set.Add(1);

        Assert.True(set.Contains(1));

        int[] array = new int[3];
        set.CopyTo(array, 2);

        Assert.Equal(new int[3] { 0, 0, 1 }, array);
    }

    [Fact]
    public void CopyToExceptions()
    {
        ORSet<int> set = new();
        int[] array = new int[2];

        set.Add(1);
        set.Add(2);

        Assert.Throws<ArgumentNullException>(() => set.CopyTo(null, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.CopyTo(array, -1));
        Assert.Throws<ArgumentException>(() => set.CopyTo(array, 1));
    }

    [Fact]
    public void SingleORSetReferenceType()
    {
        ORSet<string> set = new();

        set.Add("1");
        set.Add("2");
        Assert.True(set.Remove("1"));
        Assert.False(set.Remove("3"));

        set.Add("3");

        Assert.Equal(2, set.Count);

        Assert.Equal(new List<string> { "2", "3" }, set.LookupAll().OrderBy(t => t));

        set.Clear();
        Assert.Equal(0, set.Count);
        Assert.Equal(new List<string> { }, set.LookupAll());

        Assert.False(set.Contains("1"));

        set.Add("1");

        Assert.True(set.Contains("1"));
    }


    [Fact]
    public void SingleORSetReferenceType2()
    {
        ORSet<string> set = new();

        set.Add("1");
        set.Add("1");

        Assert.Equal(1, set.Count);

        Assert.Equal(new List<string> { "1" }, set.LookupAll());

        set.Clear();
        set.Add("");

        Assert.True(set.Contains(""));
    }

    [Fact]
    public void Multiple()
    {
        ORSet<int> set1 = new();
        ORSet<int> set2 = new();

        set1.Add(1);
        set2.Add(2);

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());

        Assert.Equal(new List<int> { 1, 2 }, set1.LookupAll());
        Assert.Equal(2, set1.Count);
        Assert.Equal(new List<int> { 2 }, set2.LookupAll());

        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());
        Assert.Equal(set1.OrderBy(t => t), set2.OrderBy(t => t));

        set1.Remove(2);
        Assert.Equal(new List<int> { 1 }, set1.LookupAll());
        Assert.Equal(1, set1.Count);

        set1.Add(2);
        set2.Remove(2);
        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
        Assert.Equal(new List<int> { 1, 2 }, set1.LookupAll().OrderBy(t => t));
        Assert.Equal(2, set1.Count);
    }

    [Fact]
    public void Multiple2()
    {
        ORSet<string> set1 = new();
        ORSet<string> set2 = new();

        set1.Add("a");

        set2.Add("a");
        set1.Remove("a");

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());

        Assert.Equal(new List<string> { "a" }, set1.LookupAll());
        Assert.Equal(1, set1.Count);
        Assert.Equal(1, set2.Count);
    }

    [Fact]
    public void Multiple3()
    {
        ORSet<int> set1 = new() { 1, 2, 3 };
        ORSet<int> set2 = new() { 1, 2 };

        set1.Remove(1);

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());

        Assert.Equal(new List<int> { 1, 2, 3 }, set1.LookupAll().OrderBy(t => t));
    }


    [Fact]
    public void Multiple4()
    {
        ORSet<int> set1 = new();
        ORSet<int> set2 = new();

        set1.Add(1);

        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());

        Assert.Equal(new List<int> { 1 }, set1.LookupAll());
        Assert.Equal(new List<int> { 1 }, set2.LookupAll());

        set1.Add(1);
        set2.Remove(1);

        Assert.Equal(new List<int> { 1 }, set1.LookupAll());
        Assert.Equal(new List<int> { }, set2.LookupAll());

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());

        Assert.Equal(new List<int> { }, set1.LookupAll());
        Assert.Equal(new List<int> { }, set2.LookupAll());
    }

    [Fact]
    public void Multiple5()
    {
        ORSet<int> set1 = new() { 1, 2, 3 };
        ORSet<int> set2 = new() { 1, 2 };
        ORSet<int> set3 = new() { 1, 2 };

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
        set1.ApplySynchronizedUpdate(set3.GetLastSynchronizedUpdate());

        set1.Remove(1);

        Assert.Equal(new List<int> { 2, 3 }, set1.LookupAll().ToList().OrderBy(t => t));
    }

    [Fact]
    public void Same()
    {
        ORSet<int> set1 = new();
        ORSet<int> set2 = new();
        ORSet<int> set3 = new();

        set1.Add(1);
        set2.Add(1);
        set3.Add(2);

        Assert.Equal(set1, set2);
        Assert.False(set1 == set2);
        Assert.True(set1.OrderBy(t => t).SequenceEqual(set2.OrderBy(t => t)));
        Assert.NotEqual(set1.GetHashCode(), set2.GetHashCode());

        Assert.NotEqual(set1, set3);
        Assert.False(set1 == set3);
        Assert.False(set1.SequenceEqual(set3));
    }

    [Fact]
    public void Same2()
    {
        ORSet<int> set1 = new();
        ORSet<int> set2 = new();
        ORSet<int> set3 = new();

        set1.Add(1);
        set1.Add(2);
        set2.Add(2);
        set2.Add(1);
        set3.Add(2);

        Assert.NotEqual(set1, set2);
        Assert.False(set1 == set2);
        Assert.Equal(set1.OrderBy(t => t), set2.OrderBy(t => t));

        Assert.True(set1.OrderBy(t => t).SequenceEqual(set2.OrderBy(t => t)));
        Assert.NotEqual(set1.GetHashCode(), set2.GetHashCode());

        Assert.NotEqual(set1, set3);
        Assert.False(set1 == set3);
        Assert.False(set1.SequenceEqual(set3));
        Assert.NotEqual(set1.OrderBy(t => t), set3.OrderBy(t => t));
    }

    [Fact]
    public void ApplySynchronizedUpdateException()
    {
        ORSet<int> set1 = new();
        ORSet<string> set2 = new();

        set1.Add(1);
        set2.Add("2");

        Assert.Throws<NotSupportedException>(() => set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate()));
    }
#nullable enable
    [Fact]
    public void AddNull()
    {
        ORSet<string?> set1 = new();
        string? name = null;

        set1.Add(name);

        Assert.Single(set1);
        Assert.Contains(null, set1);
    }

    [Fact]
    public void RemoveNull()
    {
        ORSet<string?> set1 = new();
        string? name = null;

        set1.Add(name);
        set1.Remove(name);

        Assert.Empty(set1.ToList());
    }

    [Fact]
    public void RemoveNull2()
    {
        ORSet<string?> set1 = new();
        string? name = null;

        set1.Add(name);
        set1.Add(name);
        set1.Remove(name);

        Assert.Empty(set1.ToList());
    }

    [Fact]
    public void MergeNull()
    {
        ORSet<string?> set1 = new();
        ORSet<string?> set2 = new();

        set1.Add("hi");
        set1.Add(null);

        bool val = set2.Remove(null);
        Assert.False(val);

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
        Assert.Equal(new List<string?> { "hi", null }, set1.ToList());
    }

    [Fact]
    public void MergeNull2()
    {
        ORSet<string?> set1 = new();
        ORSet<string?> set2 = new();

        set1.Add("hi");
        set1.Add(null);

        set2.Add(null);
        set2.Remove(null);

        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());
        Assert.Equal(new List<string?> { "hi", null }, set2.ToList());

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
        Assert.Equal(new List<string?> { "hi", null }, set1.ToList());
    }

    [Fact]
    public void MergeNull3()
    {
        ORSet<string?> set1 = new();
        ORSet<string?> set2 = new();

        set1.Add(null);

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());

        Assert.False(set1.Add(null));
        set2.Remove(null);

        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());
        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());

        Assert.Equal(new List<string?> { }, set2.ToList());
        Assert.Equal(new List<string?> { }, set1.ToList());
    }

    [Fact]
    public void MergeNull4()
    {
        ORSet<string?> set1 = new();
        ORSet<string?> set2 = new();

        set1.Add(null);
        set2.Add(null);

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());

        set1.Add(null);
        set2.Remove(null);

        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());
        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());

        Assert.Equal(new List<string?> { }, set2.ToList());
        Assert.Equal(new List<string?> { }, set1.ToList());
    }

    [Fact]
    public void MergeNull5()
    {
        ORSet<string?> set1 = new();
        ORSet<string?> set2 = new();

        set1.Add(null);
        set2.Add(null);

        set2.Remove(null);
        set1.Add(null);

        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());
        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());

        Assert.Equal(new List<string?> { null }, set2.ToList());
        Assert.Equal(new List<string?> { null }, set1.ToList());
    }

    [Fact]
    public void MergeNull6()
    {
        ORSet<string?> set1 = new();
        ORSet<string?> set2 = new();

        set1.Add(null);
        set2.Add(null);

        set2.Remove(null);
        set1.Add(null);

        set1.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());


        Assert.Equal(new List<string?> { null }, set2.ToList());
        Assert.Equal(new List<string?> { null }, set1.ToList());
    }

    [Fact]
    public void MergeNull7()
    {
        ORSet<string?> set1 = new();
        ORSet<string?> set2 = new();
        ORSet<string?> set3 = new();

        set1.Add(null);

        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());

        set2.Remove(null);
        set2.Add(null);
        set2.Remove(null);

        set2.ApplySynchronizedUpdate(set1.GetLastSynchronizedUpdate());

        Assert.Equal(new List<string?> { }, set2.ToList());
    }
#nullable restore

}
