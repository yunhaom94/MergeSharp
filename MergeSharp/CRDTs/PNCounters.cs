using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;

[TypeAntiEntropyProtocol(typeof(PNCounter))]
public class PNCounterMsg : PropagationMessage
{
    [JsonInclude]
    public Dictionary<Guid, int> pVector;
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


    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<PNCounterMsg>(input);
        this.pVector = json.pVector;
        this.nVector = json.nVector;
    }
    
    
    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}


[ReplicatedType("PNCounter")]
public class PNCounter : CRDT
{

    private Dictionary<Guid, int> _pVector;
    private Dictionary<Guid, int> _nVector;

    private Guid replicaIdx;

    public PNCounter()
    {
        this.replicaIdx = Guid.NewGuid();

        this._nVector = new Dictionary<Guid, int>();
        this._pVector = new Dictionary<Guid, int>();
        this._pVector[this.replicaIdx] = 0;
        this._nVector[this.replicaIdx] = 0;
    }


    public int Get()
    {
        return this._pVector.Sum(x => x.Value) - this._nVector.Sum(x => x.Value);
    }

    [OperationType(OpType.Update)]
    public virtual void Increment(int i)
    {
        this._pVector[this.replicaIdx] += i;
        //this.HasSideEffect();
    }

    [OperationType(OpType.Update)]
    public virtual void Decrement(int i)
    {
        this._nVector[this.replicaIdx] += i;
        //this.HasSideEffect();
    }
    


    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new PNCounterMsg(this._pVector, this._nVector);
    }

    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        PNCounterMsg received = (PNCounterMsg)ReceivedUpdate;
        this.Merge(received);

    }

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

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        PNCounterMsg msg = new();
        msg.Decode(input);
        return msg;
    }
}

