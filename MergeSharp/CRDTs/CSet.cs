using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;

[TypeAntiEntropyProtocol(typeof(CSet<>))]
public class CSetMsg<T> : PropagationMessage
{
    [JsonInclude]
    public Dictionary<T, int> addCount { get; private set; }

    [JsonInclude]
    public Dictionary<T, int> removeCount { get; private set; }

    public CSetMsg() { }

    public CSetMsg(Dictionary<T, int> addCount, Dictionary<T, int> removeCount)
    {
        this.addCount = addCount;
        this.removeCount = removeCount;
    }

    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<CSetMsg<T>>(input);
        this.addCount = json.addCount;
        this.removeCount = json.removeCount;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

[ReplicatedType("CSet")]
public class CSet<T> : CRDT, ICollection<T>
{
    private readonly Dictionary<T, int> _addCount;
    private readonly Dictionary<T, int> _removeCount;

    public int Count => this.LookupAll().Count();

    public bool IsReadOnly => false;

    public CSet()
    {
        this._addCount = new Dictionary<T, int>();
        this._removeCount = new Dictionary<T, int>();
    }

    [OperationType(OpType.Update)]
    public virtual void Add(T item)
    {
        _ = this._removeCount.TryGetValue(item, out int rmCount);

        if (this._addCount.TryGetValue(item, out int aCount))
        {
            int add = Math.Max(rmCount - aCount + 1, 1);
            this._addCount[item] += add;
        }
        else
        {
            this._addCount.Add(item, 1);
        }
    }

    [OperationType(OpType.Update)]
    public virtual bool Remove(T item)
    {
        if (!this._addCount.ContainsKey(item))
        {
            return false;
        }

        if (this._removeCount.ContainsKey(item))
        {
            this._removeCount[item] += 1;
        }
        else
        {
            this._removeCount.Add(item, 1);
        }

        return true;
    }

    [OperationType(OpType.Update)]
    public virtual void Clear()
    {
        foreach (var kv in this._addCount)
        {
            if (this._removeCount.TryGetValue(kv.Key, out int rmCount))
            {
                this._removeCount[kv.Key] = Math.Max(rmCount, kv.Value);
            }
            else
            {
                this._removeCount.Add(kv.Key, kv.Value);
            }
        }
    }

    private IEnumerable<T> LookupAll()
    {
        List<T> inSet = new();
        foreach (var kv in this._addCount)
        {
            _ = this._removeCount.TryGetValue(kv.Key, out int rmCount);

            if (kv.Value - rmCount > 0)
            {
                inSet.Add(kv.Key);
            }
        }

        return inSet;
    }

    public bool Contains(T item)
    {
        return this.LookupAll().Contains(item);
    }

    public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
    {
        if (receivedUpdate is not CSetMsg<T>)
        {
            throw new NotSupportedException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} does not support type of {receivedUpdate.GetType()}");
        }

        CSetMsg<T> received = (CSetMsg<T>) receivedUpdate;

        foreach (var kv in received.addCount)
        {
            if (this._addCount.TryGetValue(kv.Key, out int aCount))
            {
                this._addCount[kv.Key] = Math.Max(aCount, kv.Value);
            }
            else
            {
                this._addCount.Add(kv.Key, kv.Value);
            }
        }

        foreach (var kv in received.removeCount)
        {
            if (this._removeCount.TryGetValue(kv.Key, out int rCount))
            {
                this._removeCount[kv.Key] = Math.Max(rCount, kv.Value);
            }
            else
            {
                this._removeCount.Add(kv.Key, kv.Value);
            }
        }
    }

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        CSetMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new CSetMsg<T>(this._addCount, this._removeCount);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        this.LookupAll().ToArray().CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this.LookupAll().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
