// implementation details:
// https://en.wikipedia.org/wiki/Conflict-free_replicated_data_type#LWW-Element-Set_(Last-Write-Wins-Element-Set)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;

[TypeAntiEntropyProtocol(typeof(LWWSet<>))]
public class LWWSetMsg<T> : PropagationMessage
{
    [JsonInclude]
    public Dictionary<T, DateTime> addSet { get; private set; }

    [JsonInclude]
    public Dictionary<T, DateTime> removeSet { get; private set; }

    [JsonInclude]
    public bool isNullAdded { get; private set; }
    [JsonInclude]
    public bool isNullRemoved { get; private set; }
    [JsonInclude]
    public DateTime nullAddTime { get; private set; }
    [JsonInclude]
    public DateTime nullRemoveTime { get; private set; }


    public LWWSetMsg() { }

    public LWWSetMsg(Dictionary<T, DateTime> addSet, Dictionary<T, DateTime> removeSet,
                        bool isNullAdded, bool isNullRemoved,
                        DateTime nullAddTime, DateTime nullRemoveTime)
    {
        this.addSet = addSet;
        this.removeSet = removeSet;
        this.isNullAdded = isNullAdded;
        this.isNullRemoved = isNullRemoved;
        this.nullAddTime = nullAddTime;
        this.nullRemoveTime = nullRemoveTime;
    }


    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<LWWSetMsg<T>>(input);
        this.addSet = json.addSet;
        this.removeSet = json.removeSet;
        this.isNullAdded = json.isNullAdded;
        this.isNullRemoved = json.isNullRemoved;
        this.nullAddTime = json.nullAddTime;
        this.nullRemoveTime = json.nullRemoveTime;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

[ReplicatedType("LWWSet")]
public class LWWSet<T> : CRDT, ICollection<T>
{
    private readonly Dictionary<T, DateTime> addSet;
    private readonly Dictionary<T, DateTime> removeSet;

    private DateTime nullAddTime;
    private DateTime nullRemoveTime;
    private bool isNullAdded;
    private bool isNullRemoved;

    public int Count => this.LookupAll().Count();

    public bool IsReadOnly => false;

    public LWWSet()
    {
        this.addSet = new Dictionary<T, DateTime>();
        this.removeSet = new Dictionary<T, DateTime>();
    }

    [OperationType(OpType.Update)]
    public virtual void Add(T item)
    {
        DateTime now = DateTime.UtcNow;

        if (item is null)
        {
            this.nullAddTime = now;
            this.isNullAdded = true;
        }
        else
        {
            this.addSet[item] = now;
        }
    }

    [OperationType(OpType.Update)]
    public virtual bool Remove(T item)
    {
        DateTime now = DateTime.UtcNow;

        if (item is null)
        {
            if (this.isNullAdded)
            {
                this.nullRemoveTime = now;
                this.isNullRemoved = true;
                return true;
            }
            return false;
        }
        else
        {
            if (this.Contains(item))
            {
                this.removeSet[item] = now;
                return true;
            }
            return false;
        }
    }

    [OperationType(OpType.Update)]
    public virtual void Clear()
    {
        this.addSet.Clear();
        this.removeSet.Clear();
        this.nullAddTime = new DateTime();
        this.isNullRemoved = false;
        this.nullRemoveTime = new DateTime();
        this.isNullAdded = false;
    }

    private IEnumerable<T> LookupAll()
    {
        var onlyInAdd = this.addSet.Keys.Except(this.removeSet.Keys);

        // Note: favours add in case of a tie in Time
        var addTimeGTRmTime =
            from add in this.addSet
            join remove in this.removeSet
                on add.Key equals remove.Key
            where add.Value >= remove.Value
            select add.Key;

        var nullVal = new List<T>();

        if ((this.isNullAdded && this.isNullRemoved && this.nullAddTime >= this.nullRemoveTime) ||
            (this.isNullAdded && !this.isNullRemoved))
        {
            nullVal.Add(default(T)); // default(T) is null here
        }

        return onlyInAdd.Union(addTimeGTRmTime).Union(nullVal);
    }

    public bool Contains(T item)
    {
        return this.LookupAll().Contains(item);
    }

    public void CopyTo(T[] array, int index)
    {
        this.LookupAll().ToArray().CopyTo(array, index);
    }

    public override void ApplySynchronizedUpdate(PropagationMessage receivedUpdate)
    {
        if (receivedUpdate is not LWWSetMsg<T>)
        {
            throw new NotSupportedException($"ReceivedUpdate does not support type of {receivedUpdate.GetType()}");
        }


        LWWSetMsg<T> received = (LWWSetMsg<T>) receivedUpdate;

        foreach (KeyValuePair<T, DateTime> r in received.addSet)
        {
            if (!this.addSet.ContainsKey(r.Key))
            {
                this.addSet[r.Key] = r.Value;
            }
            else if (this.addSet[r.Key] < r.Value)
            {
                this.addSet[r.Key] = r.Value;
            }
        }

        foreach (KeyValuePair<T, DateTime> r in received.removeSet)
        {
            if (!this.removeSet.ContainsKey(r.Key))
            {
                this.removeSet[r.Key] = r.Value;
            }
            else if (this.removeSet[r.Key] < r.Value)
            {
                this.removeSet[r.Key] = r.Value;
            }
        }

        if (received.isNullAdded)
        {
            this.isNullAdded = received.isNullAdded;
            this.nullAddTime = this.nullAddTime > received.nullAddTime ? this.nullAddTime : received.nullAddTime;
        }

        if (received.isNullRemoved)
        {
            this.isNullRemoved = received.isNullRemoved;
            this.nullRemoveTime = this.nullRemoveTime > received.nullRemoveTime ? this.nullRemoveTime : received.nullRemoveTime;
        }
    }

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        LWWSetMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new LWWSetMsg<T>(this.addSet, this.removeSet,
            this.isNullAdded, this.isNullRemoved,
            this.nullAddTime, this.nullRemoveTime
        );
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
