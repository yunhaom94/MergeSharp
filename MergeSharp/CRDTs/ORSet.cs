using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;

/// <summary>
/// Propagation Message for the <c>ORSet{T}</c> class.
/// </summary>
/// <typeparam name="T">The type stored by the OR-Set</typeparam>
[TypeAntiEntropyProtocol(typeof(ORSet<>))]
public class ORSetMsg<T> : PropagationMessage
{
    /// <summary>
    /// Represents the <c>ORSet{T}</c>'s add set.
    /// </summary>
    [JsonInclude]
    public Dictionary<T, HashSet<Guid>> addSet { get; private set; }

    /// <summary>
    /// Represents the <c>ORSet{T}</c>'s remove set.
    /// </summary>
    /// <value></value>
    [JsonInclude]
    public Dictionary<T, HashSet<Guid>> removeSet { get; private set; }

    /// <summary>
    /// Represents the <c>ORSet{T}</c>'s added null values set.
    /// </summary>
    [JsonInclude]
    public HashSet<Guid> nullAddGuid { get; private set; }

    /// <summary>
    /// Represents the <c>ORSet{T}</c>'s removed null values set.
    /// </summary>
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

    /// <inheritdoc />
    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<ORSetMsg<T>>(input);
        this.addSet = json.addSet;
        this.removeSet = json.removeSet;
        this.nullAddGuid = json.nullAddGuid;
        this.nullRemoveGuid = json.nullRemoveGuid;
    }

    /// <inheritdoc />
    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

/// <summary>
/// Class <c>ORSet{T}</c> models an Observed-Remove Set.
/// </summary>
/// <typeparam name="T"> The type stored by the OR-Set.</typeparam>
[ReplicatedType("ORSet")]
public class ORSet<T> : CRDT, ICollection<T>
{
    /// <summary>
    /// Dictionary{T, Hashset{Guid}} used to track how many times a value has been added to the set.
    /// </summary>
    private readonly Dictionary<T, HashSet<Guid>> addSet;

    /// <summary>
    /// Dictionary{T, Hashset{Guid}} used to track how many times a value has been removed from the set.
    /// </summary>
    private readonly Dictionary<T, HashSet<Guid>> removeSet;

    /// <summary>
    /// Dictionary{T, Hashset{Guid}} used to track how many times a null value has been added to the set.
    /// </summary>
    private readonly HashSet<Guid> nullAddGuid;

    /// <summary>
    /// Dictionary{T, Hashset{Guid}} used to track how many times a null value has been removed from the set.
    /// </summary>
    private readonly HashSet<Guid> nullRemoveGuid;

    /// <summary>
    /// The number of elements in the <c>ORSet{T}</c>
    /// </summary>
    public int Count => this.LookupAll().Count();

    /// <summary>
    /// Gets a value indicating whether the <c>ORSet{T}</c> is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    public ORSet()
    {
        this.addSet = new Dictionary<T, HashSet<Guid>>();
        this.removeSet = new Dictionary<T, HashSet<Guid>>();
        this.nullAddGuid = new HashSet<Guid>();
        this.nullRemoveGuid = new HashSet<Guid>();
    }

    /// <summary>
    /// Adds an item to the <c>ORSet{T}</c>.
    /// </summary>
    /// <param name="item">item to be added.</param>
    [OperationType(OpType.Update)]
    void ICollection<T>.Add(T item)
    {
        _ = this.Add(item);
    }

    /// <summary>
    /// Adds an item to the <c>ORSet{T}</c>.
    /// </summary>
    /// <param name="item">item to be added.</param>
    /// <returns>Boolean indication whether the addition of the item was successful or not.</returns>
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
            if (this.addSet.ContainsKey(item))
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

    /// <summary>
    /// Removes an item from the <c>ORSet{T}</c> if it exists in the set.
    /// </summary>
    /// <param name="item">item to be removed.</param>
    /// <returns>Boolean indicating whether the removal of the item was successful or not.</returns>
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

    /// <summary>
    /// Clears the <c>ORSet{T}</c>.
    /// </summary>
    [OperationType(OpType.Update)]
    public virtual void Clear()
    {
        this.addSet.Clear();
        this.removeSet.Clear();
        this.nullAddGuid.Clear();
        this.nullRemoveGuid.Clear();
    }

    /// <summary>
    /// Returns the elements of the OR-Set.
    /// </summary>
    /// <returns>An Enumerable of the elements in the OR-Set</returns>
    public IEnumerable<T> LookupAll()
    {
        // get the result set by looking at Guid's of removeSet and compare
        // with addSet Guids. If a guid is in both sets, do not include
        // the key value pair to the result set
        // https://stackoverflow.com/questions/25735132/getkey-and-value-by-except-on-just-value

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

        return inOnlyAdd.Union(inBothSets).Union(nullVal).ToList();
    }

    /// <summary>
    /// Determines if a given value is in the set.
    /// </summary>
    /// <param name="item">Item to find in the set.</param>
    /// <returns>Boolean indicating if the given item is in the set.</returns>
    public bool Contains(T item)
    {
        return this.LookupAll().Contains(item);
    }

    /// <summary>
    /// Copies all the elements of the OR-Set to the specified one-dimensional array.
    /// </summary>
    /// <param name="array">one-dimensional array that the values wil be copied to.</param>
    /// <param name="arrayIndex">integer that indicates the index in the array where copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        this.LookupAll().ToArray().CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Merges the received OR-Set into this OR-Set.
    /// </summary>
    /// <param name="received">OR-Set that will be used for merging.</param>
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

    /// <inheritdoc />
    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        if (ReceivedUpdate is not ORSetMsg<T>)
        {
            throw new NotSupportedException($"ReceivedUpdate does not support type of {ReceivedUpdate.GetType()}");
        }
        ORSetMsg<T> received = (ORSetMsg<T>) ReceivedUpdate;
        this.Merge(received);
    }

    /// <inheritdoc />
    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        ORSetMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    /// <inheritdoc />
    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new ORSetMsg<T>(this.addSet, this.removeSet, this.nullAddGuid, this.nullRemoveGuid);
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
    /// Exposes an enumerator, which supports a simple iteration over a non-generic collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}
