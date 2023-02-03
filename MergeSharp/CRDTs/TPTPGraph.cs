using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;

[TypeAntiEntropyProtocol(typeof(TPTPGraph))]

public class TPTPGraphMsg : PropagationMessage
{
    [JsonInclude]
    public TPSetMsg<Guid> _verticesMsg;

    [JsonInclude]
    public TPSetMsg<(Guid, Guid)> _edgesMsg;

    public TPTPGraphMsg()
    {
    }

    public TPTPGraphMsg(TPSet<Guid> vertices, TPSet<(Guid, Guid)> edges)
    {
        this._verticesMsg = (TPSetMsg<Guid>) vertices.GetLastSynchronizedUpdate();
        this._edgesMsg = (TPSetMsg<(Guid, Guid)>) edges.GetLastSynchronizedUpdate();
    }

    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<TPTPGraphMsg>(input);
        this._verticesMsg = json._verticesMsg;
        this._edgesMsg = json._edgesMsg;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

[ReplicatedType("TPTPGraph")]
public class TPTPGraph : CRDT
{
    private readonly TPSet<Guid> _vertices;
    private readonly TPSet<(Guid, Guid)> _edges;

    public TPTPGraph()
    {
        this._vertices = new TPSet<Guid>();
        this._edges = new TPSet<(Guid, Guid)>();
    }

    [OperationType(OpType.Update)]
    public virtual void AddVertex(Guid v)
    {
        this._vertices.Add(v);
    }

    [OperationType(OpType.Update)]
    public virtual bool RemoveVertex(Guid v)
    {
        var firstEdges = this.LookupEdges().Select(e => e.Item1);
        var secondEdges = this.LookupEdges().Select(e => e.Item2);

        // the vertex is in the set and the vertex does not support any active edges
        if (this.Contains(v) && !firstEdges.Contains(v) && !secondEdges.Contains(v))
        {
            return this._vertices.Remove(v);
        }
        return false;
    }

    [OperationType(OpType.Update)]
    public virtual bool AddEdge(Guid v1, Guid v2)
    {
        var vertices = this.LookupVertices();

        if (!vertices.Contains(v1) || !vertices.Contains(v2)) {
            return false;
        }

        this._edges.Add((v1, v2));
        return true;
    }

    [OperationType(OpType.Update)]
    public virtual bool RemoveEdge(Guid v1, Guid v2)
    {
        return this._edges.Remove((v1, v2));
    }


    public IEnumerable<(Guid, Guid)> LookupEdges()
    {
        List<(Guid, Guid)> edges = new();
        List<(Guid, Guid)> existingEdges = this._edges.LookupAll();
        HashSet<Guid> existingVertices = this.LookupVertices().ToHashSet();

        foreach ((Guid, Guid) vertices in existingEdges)
        {
            if (existingVertices.Contains(vertices.Item1) && existingVertices.Contains(vertices.Item2))
            {
                edges.Add(vertices);
            }
        }

        return edges;
    }

    public IEnumerable<Guid> LookupVertices()
    {
        return this._vertices.LookupAll();
    }

    public bool Contains(Guid v)
    {
        return this.LookupVertices().Contains(v);
    }

    public bool Contains(Guid v1, Guid v2)
    {
        return this.LookupEdges().Contains((v1, v2));
    }



    public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate){
        if (receivedUpdate is not TPTPGraphMsg)
        {
            throw new NotSupportedException($"ApplySynchronizedUpdate does not support receivedUpdate type of {receivedUpdate.GetType()}");
        }

        TPTPGraphMsg received = (TPTPGraphMsg) receivedUpdate;
        this._edges.ApplySynchronizedUpdate(received._edgesMsg);
        this._vertices.ApplySynchronizedUpdate(received._verticesMsg);
    }

    public override PropagationMessage DecodePropagationMessage(byte[] input) {
        TPTPGraphMsg msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate() {
        return new TPTPGraphMsg(this._vertices, this._edges);
    }
}
