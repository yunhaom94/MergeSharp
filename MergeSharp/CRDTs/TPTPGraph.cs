using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;

/// <summary>
/// The <c>PropagationMessage</c> for <c>TPTPGraph</c> Class.
/// </summary>
[TypeAntiEntropyProtocol(typeof(TPTPGraph))]
public class TPTPGraphMsg : PropagationMessage
{
    /// <summary>
    /// <c>TPSetMsg</c> containing <c>Guid</c> of vertices.
    /// </summary>
    [JsonInclude]
    public TPSetMsg<Guid> _verticesMsg;

    /// <summary>
    /// <c>TPSetMsg</c> containing <c>{Guid, Guid}</c> of edges.
    /// </summary>
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

    /// <inheritdoc/>
    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<TPTPGraphMsg>(input);
        this._verticesMsg = json._verticesMsg;
        this._edgesMsg = json._edgesMsg;
    }

    /// <inheritdoc/>
    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

/// <summary>
/// Two-Phase Two-Phase Graph. Semantics restrict readdition of vertices and edges after removal.
/// </summary>
[ReplicatedType("TPTPGraph")]
public class TPTPGraph : CRDT
{
    /// <summary>
    /// <c>TPSet</c> containing <c>Guid</c> of vertices.
    /// </summary>
    private readonly TPSet<Guid> _vertices;

    /// <summary>
    /// <c>TPSet</c> containing <c>Guid</c> of edges.
    /// </summary>
    private readonly TPSet<(Guid, Guid)> _edges;

    public TPTPGraph()
    {
        this._vertices = new TPSet<Guid>();
        this._edges = new TPSet<(Guid, Guid)>();
    }

    /// <summary>
    /// Method to add a vertex.
    /// </summary>
    /// <param name="v"><c>Guid</c> of vertex to be added.</param>
    [OperationType(OpType.Update)]
    public virtual void AddVertex(Guid v)
    {
        this._vertices.Add(v);
    }

    /// <summary>
    /// Method to remove a vertex.
    /// </summary>
    /// <param name="v"><c>Guid</c> of vertex to be removed.</param>
    /// <returns><c>true</c> if successfully added, otherwise <c>false</c>.</returns>
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

    /// <summary>
    /// Method to add an edge.
    /// </summary>
    /// <param name="v1"><c>Guid</c> of source vertex of the edge.</param>
    /// <param name="v2"><c>Guid</c> of destination vertex of the edge.</param>
    /// <returns><c>true</c> if successfully added, otherwise <c>false</c>.</returns>
    [OperationType(OpType.Update)]
    public virtual bool AddEdge(Guid v1, Guid v2)
    {
        var vertices = this.LookupVertices();

        if (!vertices.Contains(v1) || !vertices.Contains(v2))
        {
            return false;
        }

        this._edges.Add((v1, v2));
        return true;
    }

    /// <summary>
    /// Method to remove an edge.
    /// </summary>
    /// <param name="v1"><c>Guid</c> of source vertex of the edge.</param>
    /// <param name="v2"><c>Guid</c> of destination vertex of the edge.</param>
    /// <returns><c>true</c> if successfully added, otherwise <c>false</c>.</returns>
    [OperationType(OpType.Update)]
    public virtual bool RemoveEdge(Guid v1, Guid v2)
    {
        return this._edges.Remove((v1, v2));
    }

    /// <summary>
    /// Method that returns all edges in the <c>TPTPGraph</c>.
    /// </summary>
    /// <returns><c>IEnumerable{(Guid, Guid)}</c> of edges.</returns>
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

    /// <summary>
    /// Method that returns all vertices in the <c>TPTPGraph</c>.
    /// </summary>
    /// <returns><c>IEnumerable{Guid}</c> of vertices.</returns>
    public IEnumerable<Guid> LookupVertices()
    {
        return this._vertices.LookupAll();
    }

    /// <summary>
    /// Method that returns if a vertex is in the <c>TPTPGraph</c>.
    /// </summary>
    /// <param name="v"><c>Guid</c> of vertex to be checked.</param>
    /// <returns><c>true</c> if the vertex is in the <c>TPTPGraph</c>, otherwise <c>false</c>.</returns>
    public bool Contains(Guid v)
    {
        return this.LookupVertices().Contains(v);
    }

    /// <summary>
    /// Method that returns if an edge is in the <c>TPTPGraph</c>.
    /// </summary>
    /// <param name="v1"><c>Guid</c> of source vertex of the edge.</param>
    /// <param name="v2"><c>Guid</c> of destination vertex of the edge.</param>
    /// <returns><c>true</c> if the edge is in the <c>TPTPGraph</c>, otherwise <c>false</c>.</returns>
    public bool Contains(Guid v1, Guid v2)
    {
        return this.LookupEdges().Contains((v1, v2));
    }

    /// <inheritdoc/>
    public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
    {
        if (receivedUpdate is not TPTPGraphMsg)
        {
            throw new NotSupportedException($"ApplySynchronizedUpdate does not support receivedUpdate type of {receivedUpdate.GetType()}");
        }

        TPTPGraphMsg received = (TPTPGraphMsg) receivedUpdate;
        this._edges.ApplySynchronizedUpdate(received._edgesMsg);
        this._vertices.ApplySynchronizedUpdate(received._verticesMsg);
    }

    /// <inheritdoc/>
    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        TPTPGraphMsg msg = new();
        msg.Decode(input);
        return msg;
    }

    /// <inheritdoc/>
    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new TPTPGraphMsg(this._vertices, this._edges);
    }
}
