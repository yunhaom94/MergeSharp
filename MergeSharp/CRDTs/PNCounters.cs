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
    /// The class member for the positive values of the <c>PNCounterMsg</c>.
    /// </summary>
    [JsonInclude]
    public Dictionary<Guid, int> pVector;

    /// <summary>
    /// The class member for the negative values of the <c>PNCounterMsg</c>.
    /// </summary>
    [JsonInclude]
    public Dictionary<Guid, int> nVector;

    public PNCounterMsg()
    {
    }

    /// <summary>
    /// Parametrized constructor for <c>PNCounterMsg</c>.
    /// </summary>
    /// <param name="pVector"><c>Dictionary</c> of {Guid, int} pairs containing the positive values.</param>
    /// <param name="nVector"><c>Dictionary</c> of {Guid, int} pairs containing the negative values.</param>
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
/// The <c>PNCounter</c> Class.
/// </summary>
[ReplicatedType("PNCounter")]
public class PNCounter : CRDT
{
    /// <summary>
    /// Member for positive values of the <c>PNCounter</c>.
    /// </summary>
    private Dictionary<Guid, int> _pVector;

    /// <summary>
    /// Member for negative values of the <c>PNCounter</c>.
    /// </summary>
    private Dictionary<Guid, int> _nVector;

    /// <summary>
    /// The class member for the replica ID of the <c>PNCounter</c>.
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
    /// <param name="i">Value to increment the <c>PNCounter</c> by.</param>
    [OperationType(OpType.Update)]
    public virtual void Increment(int i)
    {
        this._pVector[this.replicaIdx] += i;
        //this.HasSideEffect();
    }

    /// <summary>
    /// Method to decrement the <c>PNCounter</c>.
    /// </summary>
    /// <param name="i">Value to decrement the <c>PNCounter</c> by.</param>
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

