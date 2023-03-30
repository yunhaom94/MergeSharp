// implementation details:
// https://en.wikipedia.org/wiki/Conflict-free_replicated_data_type#LWW-Element-Set_(Last-Write-Wins-Element-Set)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;

/// <summary>
/// PropagationMessage for the <c>LWWSetMsg{T}</c>.
/// </summary>
/// <typeparam name="T">Type which the <c>LWWSetMsg{T}</c> holds.</typeparam>
[TypeAntiEntropyProtocol(typeof(LWWSet<>))]
public class LWWSetMsg<T> : PropagationMessage
{
    /// <summary>
    /// Dictionary of values <c>T</c> and the latest <c>DateTime</c>s they were added to the <c>TPSet{T}</c>.
    /// </summary>
    [JsonInclude]
    public Dictionary<T, DateTime> addSet { get; private set; }

    /// <summary>
    /// Dictionary of values <c>T</c> and the latest <c>DateTime</c>s they were removed from the <c>TPSet{T}</c>.
    /// </summary>
    [JsonInclude]
    public Dictionary<T, DateTime> removeSet { get; private set; }

    /// <summary>
    /// Is the value <c>null</c> added to the <c>LWWSet{T}</c>.
    /// </summary>
    [JsonInclude]
    public bool isNullAdded { get; private set; }

    /// <summary>
    /// Is the value <c>null</c> removed from the <c>LWWSet{T}</c>.
    /// </summary>
    [JsonInclude]
    public bool isNullRemoved { get; private set; }

    /// <summary>
    /// Latest <c>DateTime</c> the value <c>null</c> was added to the <c>LWWSet{T}</c>.
    /// </summary>
    [JsonInclude]
    public DateTime nullAddTime { get; private set; }

    /// <summary>
    /// Latest <c>DateTime</c> the value <c>null</c> was removed from the <c>LWWSet{T}</c>.
    /// </summary>
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

/// <summary>
/// Last Writer Wins Set. Semantics follow that the last write for a value wins.
/// </summary>
/// <typeparam name="T">Type which the <c>LWWSet{T}</c> holds.</typeparam>
[ReplicatedType("LWWSet")]
public class LWWSet<T> : CRDT, ICollection<T>
{
    /// <summary>
    /// Dictionary of values <c>T</c> and the latest <c>DateTime</c>s they were added to the <c>TPSet{T}</c>.
    /// </summary>
    private readonly Dictionary<T, DateTime> addSet;

    /// <summary>
    /// Dictionary of values <c>T</c> and the latest <c>DateTime</c>s they were removed from the <c>TPSet{T}</c>.
    /// </summary>
    private readonly Dictionary<T, DateTime> removeSet;

    /// <summary>
    /// Latest <c>DateTime</c> the value <c>null</c> was added to the <c>LWWSet{T}</c>.
    /// </summary>
    private DateTime nullAddTime;

    /// <summary>
    /// Latest <c>DateTime</c> the value <c>null</c> was removed from the <c>LWWSet{T}</c>.
    /// </summary>
    private DateTime nullRemoveTime;

    /// <summary>
    /// Is the value <c>null</c> added to the <c>LWWSet{T}</c>.
    /// </summary>
    private bool isNullAdded;

    /// <summary>
    /// Is the value <c>null</c> removed from the <c>LWWSet{T}</c>.
    /// </summary>
    private bool isNullRemoved;

    /// <summary>
    /// Count of elements in the <c>LWWSet{T}</c>.
    /// </summary>
    public int Count => this.LookupAll().Count();

    /// <summary>
    /// Get a value indicating whether the <c>LWWSet{T}</c> is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    public LWWSet()
    {
        this.addSet = new Dictionary<T, DateTime>();
        this.removeSet = new Dictionary<T, DateTime>();
    }

    /// <summary>
    /// Add element <c>item</c> to the <c>LWWSet{T}</c> with the current <c>DateTime</c>.
    /// </summary>
    /// <param name="item">Element to add.</param>
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

    /// <summary>
    /// Remove element <c>item</c> from the <c>LWWSet{T}</c> with the current <c>DateTime</c>.
    /// </summary>
    /// <param name="item">Element to remove.</param>
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

    /// <summary>
    /// Removes all items from the <c>LWWSet{T}</c>.
    /// </summary>
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

    /// <summary>
    /// Queries all elements in the <c>LWWSet{T}</c>.
    /// </summary>
    /// <returns><c>IEnumerable</c> of elements in the <c>LWWSet{T}</c>.</returns>
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

    /// <summary>
    /// Checks if <c>LWWset{T}</c> contains element.
    /// </summary>
    /// <param name="item">Element to check if contained in <c>LWWSet{T}</c>.</param>
    /// <returns><c>true</c> if the element is in the <c>LWWset{T}</c>, <c>false</c> otherwise.</returns>
    public bool Contains(T item)
    {
        return this.LookupAll().Contains(item);
    }

    /// <summary>
    /// Copies all the elements of the current <c>LWWSet{T}</c> to the specified one-dimensional array.
    /// </summary>
    /// <param name="array">Target array to copy to.</param>
    /// <param name="arrayIndex">Index of <c>array</c> at which to begin copying.</param>
    public void CopyTo(T[] array, int index)
    {
        this.LookupAll().ToArray().CopyTo(array, index);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        LWWSetMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    /// <inheritdoc />
    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new LWWSetMsg<T>(this.addSet, this.removeSet,
            this.isNullAdded, this.isNullRemoved,
            this.nullAddTime, this.nullRemoveTime
        );
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return this.LookupAll().GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>An <c>IEnumerator</c> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
