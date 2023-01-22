using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;

[TypeAntiEntropyProtocol(typeof(CDictionary<>))]
public class CDictionaryMsg<TKey> : PropagationMessage
{
    [JsonInclude]
    public Dictionary<TKey, int> addCount { get; private set; }

    [JsonInclude]
    public Dictionary<TKey, int> removeCount { get; private set; }

    public CDictionaryMsg() { }

    public CDictionaryMsg(Dictionary<TKey, int> addCount, Dictionary<TKey, int> removeCount)
    {
        this.addCount = addCount;
        this.removeCount = removeCount;
    }

    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<CDictionaryMsg<TKey>>(input);
        this.addCount = json.addCount;
        this.removeCount = json.removeCount;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

[ReplicatedType("CDictionary")]
public class CDictionary<TKey> : CRDT, IDictionary<TKey, int>
{
    private readonly Dictionary<TKey, int> _addCount;
    private readonly Dictionary<TKey, int> _removeCount;

    public ICollection<TKey> Keys => this.LookupKeys();
    public ICollection<int> Values => this.LookupValues();

    public int Count => this.Keys.Count;
    public bool IsReadOnly => false;

    // TODO: Should this be marked with [OperationType(OpType.Update)] ?
    public int this[TKey key] {
        get {
            _ = this._removeCount.TryGetValue(key, out int rmCount);
            return this.ContainsKey(key) ? this._addCount[key] - rmCount : 0;
        }
        set {
            if (value == 0 && !this._addCount.ContainsKey(key)) {
                this._addCount.Add(key, value);
            }
            else
            {
                this.Add(key, value);
            }
        }
    }

    public CDictionary()
    {
        this._addCount = new Dictionary<TKey, int>();
        this._removeCount = new Dictionary<TKey, int>();
    }


    [OperationType(OpType.Update)]
    public virtual void Add(TKey key, int value) {
        if (value == 0)
        {
            if (!this._addCount.ContainsKey(key)) {
                this._addCount.Add(key, value);
            }
        }
        else if (value < 0)
        {
            _ = this.Remove(key, Math.Abs(value));
        }
        else
        {
            if (this._addCount.TryGetValue(key, out int aCount))
            {
                _ = this._removeCount.TryGetValue(key, out int rmCount);
                int add = Math.Max(rmCount - aCount + 1, value);
                this._addCount[key] += add;
            }
            else
            {
                this._addCount.Add(key, value);
            }
        }
    }
    [OperationType(OpType.Update)]
    public virtual void Add(KeyValuePair<TKey, int> item) => this.Add(item.Key, item.Value);
    [OperationType(OpType.Update)]
    public virtual void Add(TKey key) => this.Add(key, 1);

    [OperationType(OpType.Update)]
    public virtual bool Remove(TKey key, int value) {
        if (!this.ContainsKey(key))
        {
            return false;
        }

        if (value == 0)
        {
            return true;
        }
        else if (value < 0) {
            this.Add(key, Math.Abs(value));
            return true;
        }
        else
        {
            if (this._addCount.TryGetValue(key, out int aCount)) {
                bool inRmCount = this._removeCount.TryGetValue(key, out int rmCount);

                int diff = aCount - rmCount;
                if (diff <= 0 )
                {
                    return false;
                }

                if (inRmCount)
                {
                    this._removeCount[key] += diff;
                }
                else
                {
                    this._removeCount.Add(key, diff);
                }

                return true;
            }
            else {
                return false;
            }
        }
    }
    [OperationType(OpType.Update)]
    public virtual bool Remove(KeyValuePair<TKey, int> item) => this.Remove(item.Key, item.Value);
    [OperationType(OpType.Update)]
    public virtual bool Remove(TKey key) => this.Remove(key, 1);

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


    private ICollection<TKey> LookupKeys()
    {
        List<TKey> result = new();
        foreach (var kv in this._addCount)
        {
            _ = this._removeCount.TryGetValue(kv.Key, out int rmCount);

            int diff = kv.Value - rmCount;
            if (diff > 0)
            {
                result.Add(kv.Key);
            }
        }

        return result;
    }

    private ICollection<int> LookupValues()
    {
        List<int> result = new();
        foreach (var kv in this._addCount)
        {
            _ = this._removeCount.TryGetValue(kv.Key, out int rmCount);

            int diff = kv.Value - rmCount;
            if (diff > 0)
            {
                result.Add(diff);
            }
        }

        return result;
    }


    public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
    {
        if (receivedUpdate is not CDictionaryMsg<TKey>)
        {
            throw new NotSupportedException($"{System.Reflection.MethodBase.GetCurrentMethod().Name} does not support type of {receivedUpdate.GetType()}");
        }

        CDictionaryMsg<TKey> received = (CDictionaryMsg<TKey>) receivedUpdate;

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
        CDictionaryMsg<TKey> msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new CDictionaryMsg<TKey>(this._addCount, this._removeCount);
    }


    public bool Contains(KeyValuePair<TKey, int> item) => this.ContainsKey(item.Key) && this[item.Key] == item.Value;

    public bool ContainsKey(TKey key) => this.Keys.Contains(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out int value) {
        if (this.ContainsKey(key)) {
            value = this[key];
            return true;
        }

        value = 0;
        return false;
    }


    public void CopyTo(KeyValuePair<TKey, int>[] array, int arrayIndex) => throw new NotImplementedException();

    public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator() => (IEnumerator<KeyValuePair<TKey, int>>) this.Keys.Zip(this.Values);
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}
