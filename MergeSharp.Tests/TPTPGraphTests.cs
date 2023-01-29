using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MergeSharp.Tests;

public class TPTPGraphTests
{
    [Fact]
    public void SingleGraph()
    {
        TPTPGraph graph = new();
        var v1 = Guid.NewGuid();
        var v2 = Guid.NewGuid();
        Assert.False(graph.AddEdge(v1, v2));
        Assert.Empty(graph.LookupEdges());

        graph.AddVertex(v1);
        Assert.Equal(graph.LookupVertices(), new[] { v1 });

        Assert.False(graph.AddEdge(v1, v2));
        Assert.False(graph.AddEdge(v2, v1));

        graph.AddVertex(v2);
        Assert.Equal(graph.LookupVertices().ToHashSet(), new HashSet<Guid> { v1, v2 });

        Assert.True(graph.AddEdge(v1, v2));
        Assert.True(graph.AddEdge(v1, v2));
        Assert.True(graph.AddEdge(v1, v1));
        Assert.True(graph.AddEdge(v2, v1));

        Assert.Equal(graph.LookupEdges().ToHashSet(),
                        new HashSet<(Guid, Guid)> {
                            (v1, v2),
                            (v1, v1),
                            (v2, v1)
                         });

        Assert.False(graph.RemoveVertex(v1));

        Assert.False(graph.RemoveEdge(v2, v2));
        Assert.True(graph.RemoveEdge(v1, v2));
        Assert.False(graph.RemoveEdge(v1, v2));
        Assert.True(graph.RemoveEdge(v2, v1));

        Assert.False(graph.RemoveVertex(v1));
        Assert.True(graph.RemoveVertex(v2));
    }

    [Fact]
    public void MultipleGraphs1()
    {
        var v1 = Guid.NewGuid();
        var v2 = Guid.NewGuid();

        TPTPGraph first = new();
        TPTPGraph second = new();

        first.AddVertex(v1);
        first.AddVertex(v2);

        second.ApplySynchronizedUpdate(first.GetLastSynchronizedUpdate());

        first.RemoveVertex(v1);
        second.AddEdge(v1, v2);

        Assert.Equal(new[] { v2 }, first.LookupVertices());
        Assert.Empty(first.LookupEdges());

        Assert.Equal(new HashSet<Guid> { v1, v2 }, second.LookupVertices().ToHashSet());
        Assert.Equal(new[] { (v1, v2) }, second.LookupEdges());

        first.ApplySynchronizedUpdate(second.GetLastSynchronizedUpdate());
        second.ApplySynchronizedUpdate(first.GetLastSynchronizedUpdate());

        Assert.Equal(first.LookupVertices(), second.LookupVertices());
        Assert.Equal(first.LookupEdges(), second.LookupEdges());

        Assert.Equal(new[] { v2 }, first.LookupVertices());
        Assert.Empty(first.LookupEdges());
    }

    [Fact]
    public void MultipleGraphs2()
    {
        var v1 = Guid.NewGuid();
        var v2 = Guid.NewGuid();

        TPTPGraph first = new();
        TPTPGraph second = new();

        first.AddVertex(v1);
        first.AddVertex(v2);

        second.ApplySynchronizedUpdate(first.GetLastSynchronizedUpdate());

        first.RemoveVertex(v1);
        second.AddEdge(v1, v2);

        first.ApplySynchronizedUpdate(second.GetLastSynchronizedUpdate());
        second.ApplySynchronizedUpdate(first.GetLastSynchronizedUpdate());

        second.AddVertex(v1);

        first.ApplySynchronizedUpdate(second.GetLastSynchronizedUpdate());
        second.ApplySynchronizedUpdate(first.GetLastSynchronizedUpdate());

        Assert.Equal(first.LookupVertices(), second.LookupVertices());
        Assert.Equal(first.LookupEdges(), second.LookupEdges());

        Assert.Equal(new[] { v2 }, first.LookupVertices());
        Assert.Empty(first.LookupEdges());
    }
}

public class TPTPGraphMsgTests
{
    [Fact]
    public void EncodeDecode()
    {
        TPTPGraph graph1 = new();
        var v1 = Guid.NewGuid();
        graph1.AddVertex(v1);

        TPTPGraph graph2 = new();
        var v2 = Guid.NewGuid();
        graph2.AddVertex(v2);

        var encodedMsg2 = graph2.GetLastSynchronizedUpdate().Encode();
        TPTPGraphMsg decodedMsg2 = new();
        decodedMsg2.Decode(encodedMsg2);

        graph1.ApplySynchronizedUpdate(decodedMsg2);

        Assert.Equal(graph1.LookupVertices(), new List<Guid> { v1, v2 });
    }
}
