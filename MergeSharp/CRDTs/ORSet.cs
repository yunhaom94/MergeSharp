using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;


[TypeAntiEntropyProtocol(typeof(ORSet<>))]
public class ORSetMsg<T> : PropagationMessage
{
    [JsonInclude]
    public Dictionary<T, HashSet<Guid>> addSet { get; private set; }
    [JsonInclude]
    public Dictionary<T, HashSet<Guid>> removeSet { get; private set; }

    [JsonInclude]
    public HashSet<Guid> nullAddGuid { get; private set; }
    [JsonInclude]
    public HashSet<Guid> nullRemoveGuid { get; private set; }

    public ORSetMsg()
    {
    }

    public ORSetMsg(Dictionary<T, HashSet<Guid>> addSet, Dictionary<T, HashSet<Guid>> removeSet,
                        HashSet<Guid> nullAddGuid, HashSet<Guid> nullRemoveGuid)
    {
        this.addSet = addSet;
        this.removeSet = removeSet;
        this.nullAddGuid = nullAddGuid;
        this.nullRemoveGuid = nullRemoveGuid;
    }

    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<ORSetMsg<T>>(input);
        this.addSet = json.addSet;
        this.removeSet = json.removeSet;
        this.nullAddGuid = json.nullAddGuid;
        this.nullRemoveGuid = json.nullRemoveGuid;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

[ReplicatedType("ORSet")]
public class ORSet<T> : CRDT, ICollection<T>
{
    private readonly Dictionary<T, HashSet<Guid>> addSet;
    private readonly Dictionary<T, HashSet<Guid>> removeSet;

    private readonly HashSet<Guid> nullAddGuid;
    private readonly HashSet<Guid> nullRemoveGuid;

    public int Count => this.LookupAll().Count();

    public bool IsReadOnly => false;

    public ORSet()
    {
        this.addSet = new Dictionary<T, HashSet<Guid>>();
        this.removeSet = new Dictionary<T, HashSet<Guid>>();
        this.nullAddGuid = new HashSet<Guid>();
        this.nullRemoveGuid = new HashSet<Guid>();
    }

    [OperationType(OpType.Update)]
    void ICollection<T>.Add(T item)
    {
        _ = this.Add(item);
    }

    [OperationType(OpType.Update)]
    public virtual bool Add(T item)
    {
        if (item is null)
        {
                _ = this.nullAddGuid.Add(Guid.NewGuid());
                return true;
        }
        else
        {
            if (this.addSet.ContainsKey(item) || this.Contains(item))
            {
                _ = this.addSet[item].Add(Guid.NewGuid());
            }
            else
            {
                this.addSet[item] = new HashSet<Guid> { Guid.NewGuid() };
            }
        }
        return true;
    }

    [OperationType(OpType.Update)]
    public virtual bool Remove(T item)
    {
        if (item is null && this.LookupAll().Contains(item))
        {
            this.nullRemoveGuid.UnionWith(this.nullAddGuid);
            return true;
        }
        else
        {
            if (!this.Contains(item))
            {
                return false;
            }

            IEnumerable<Guid> toRemove = this.addSet[item];
            if (this.removeSet.ContainsKey(item))
            {
                this.removeSet[item].UnionWith(toRemove);
            }
            else
            {
                this.removeSet[item] = new HashSet<Guid>(toRemove);
            }
            return true;
        }
    }

    [OperationType(OpType.Update)]
    public void Clear()
    {
        this.addSet.Clear();
        this.removeSet.Clear();
        this.nullAddGuid.Clear();
        this.nullRemoveGuid.Clear();
    }

    // get the result set by looking at Guid's of removeSet and compare
    // with addSet Guids. If a guid is in both sets, do not include
    // the key value pair to the result set
    // https://stackoverflow.com/questions/25735132/getkey-and-value-by-except-on-just-value
    public IEnumerable<T> LookupAll()
    {
        IEnumerable<T> inOnlyAdd = this.addSet.Keys.Except(this.removeSet.Keys);

        IEnumerable<T> inBothSets =
            from add in this.addSet
            join remove in this.removeSet on add.Key equals remove.Key // inner join
            where !add.Value.SetEquals(remove.Value)
            select add.Key;

        var nullVal = new List<T>();

        if (!this.nullRemoveGuid.SetEquals(this.nullAddGuid))
        {
            nullVal.Add(default); // default is null here
        }

        return inOnlyAdd.Concat(inBothSets).Concat(nullVal).ToList();
    }

    public bool Contains(T item)
    {
        return this.LookupAll().Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        this.LookupAll().ToArray().CopyTo(array, arrayIndex);
    }
    private void Merge(ORSetMsg<T> received)
    {
        foreach (KeyValuePair<T, HashSet<Guid>> r in received.addSet)
        {
            if (this.addSet.ContainsKey(r.Key))
            {
                this.addSet[r.Key].UnionWith(r.Value);
            }
            else
            {
                HashSet<Guid> newHashSetGuid = new(r.Value);
                this.addSet[r.Key] = newHashSetGuid;
            }
        }

        foreach (KeyValuePair<T, HashSet<Guid>> r in received.removeSet)
        {
            if (this.removeSet.ContainsKey(r.Key))
            {
                this.removeSet[r.Key].UnionWith(r.Value);
            }
            else
            {
                HashSet<Guid> newHashSetGuid = new(r.Value);
                this.removeSet[r.Key] = newHashSetGuid;
            }
        }

        this.nullAddGuid.UnionWith(received.nullAddGuid);
        this.nullRemoveGuid.UnionWith(received.nullRemoveGuid);
    }

    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        if (ReceivedUpdate is not ORSetMsg<T>)
        {
            throw new NotSupportedException($"ReceivedUpdate does not support type of {ReceivedUpdate.GetType()}");
        }
        ORSetMsg<T> received = (ORSetMsg<T>) ReceivedUpdate;
        this.Merge(received);
    }

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        ORSetMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new ORSetMsg<T>(this.addSet, this.removeSet, this.nullAddGuid, this.nullRemoveGuid);
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
