using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;

/// <summary>
/// The <c>PropagationMessage</c> for the <c>PNCounter</c> Class.
/// </summary>
[TypeAntiEntropyProtocol(typeof(PNCounter))]
public class PNCounterMsg : PropagationMessage
{
    /// <summary>
    /// <c>Dictionary</c> of replicas' positive count.
    /// </summary>
    [JsonInclude]
    public Dictionary<Guid, int> pVector;

    /// <summary>
    /// <c>Dictionary</c> of replicas' negative count.
    /// </summary>
    [JsonInclude]
    public Dictionary<Guid, int> nVector;

    public PNCounterMsg()
    {
    }

    public PNCounterMsg(Dictionary<Guid, int> pVector, Dictionary<Guid, int> nVector)
    {
        this.pVector = pVector;
        this.nVector = nVector;
    }

    /// <inheritdoc/>
    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<PNCounterMsg>(input);
        this.pVector = json.pVector;
        this.nVector = json.nVector;
    }

    /// <inheritdoc/>
    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

/// <summary>
/// Positive Negative Counter. Semantics allows incrementing and decrementing counter.
/// </summary>
[ReplicatedType("PNCounter")]
public class PNCounter : CRDT
{
    /// <summary>
    /// <c>Dictionary</c> of replicas' positive count.
    /// </summary>
    private Dictionary<Guid, int> _pVector;

    /// <summary>
    /// <c>Dictionary</c> of replicas' negative count.
    /// </summary>
    private Dictionary<Guid, int> _nVector;

    /// <summary>
    /// Unique replica ID of the <c>PNCounter</c>.
    /// </summary>
    private Guid replicaIdx;

    public PNCounter()
    {
        this.replicaIdx = Guid.NewGuid();

        this._nVector = new Dictionary<Guid, int>();
        this._pVector = new Dictionary<Guid, int>();
        this._pVector[this.replicaIdx] = 0;
        this._nVector[this.replicaIdx] = 0;
    }

    /// <summary>
    /// Method to get the value of the <c>PNCounter</c>.
    /// </summary>
    /// <returns>Integer value of the <c>PNCounter</c>.</returns>
    public int Get()
    {
        return this._pVector.Sum(x => x.Value) - this._nVector.Sum(x => x.Value);
    }

    /// <summary>
    /// Method to increment the <c>PNCounter</c>.
    /// </summary>
    /// <param name="i">Integer value to increment the <c>PNCounter</c> by.</param>
    [OperationType(OpType.Update)]
    public virtual void Increment(int i)
    {
        this._pVector[this.replicaIdx] += i;
        //this.HasSideEffect();
    }

    /// <summary>
    /// Method to decrement the <c>PNCounter</c>.
    /// </summary>
    /// <param name="i">Integer value to decrement the <c>PNCounter</c> by.</param>
    [OperationType(OpType.Update)]
    public virtual void Decrement(int i)
    {
        this._nVector[this.replicaIdx] += i;
        //this.HasSideEffect();
    }

    /// <inheritdoc/>
    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new PNCounterMsg(this._pVector, this._nVector);
    }

    /// <inheritdoc/>
    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        PNCounterMsg received = (PNCounterMsg) ReceivedUpdate;
        this.Merge(received);
    }

    /// <summary>
    /// Method to merge a received update into this <c>PNCounter</c>.
    /// </summary>
    /// <param name="received">The update to be merged as a <c>PNCounterMsg</c>.</param>
    public void Merge(PNCounterMsg received)
    {
        foreach (var kv in received.pVector)
        {
            this._pVector.TryGetValue(kv.Key, out int value);
            this._pVector[kv.Key] = Math.Max(value, kv.Value);
        }

        foreach (var kv in received.nVector)
        {
            this._nVector.TryGetValue(kv.Key, out int value);
            this._nVector[kv.Key] = Math.Max(value, kv.Value);
        }
    }

    /// <inheritdoc/>
    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        PNCounterMsg msg = new();
        msg.Decode(input);
        return msg;
    }
}

