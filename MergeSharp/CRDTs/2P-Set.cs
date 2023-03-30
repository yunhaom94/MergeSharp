using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;


/// <summary>
/// Propagation Message for the <c>TPSet{T}</c>.
/// </summary>
/// <typeparam name="T">Type which the <c>TPSet{T}</c> holds.</typeparam>
[TypeAntiEntropyProtocol(typeof(TPSet<>))]
public class TPSetMsg<T> : PropagationMessage
{
    /// <summary>
    /// Set of values <c>T</c> that have been added to the <c>TPSet{T}</c>.
    /// </summary>
    [JsonInclude]
    public HashSet<T> addSet;

    /// <summary>
    /// Set of values <c>T</c> that have been removed from the <c>TPSet{T}</c>.
    /// </summary>
    [JsonInclude]
    public HashSet<T> removeSet;

    public TPSetMsg()
    {
    }

    public TPSetMsg(HashSet<T> addSet, HashSet<T> removeSet)
    {
        this.addSet = addSet;
        this.removeSet = removeSet;
    }

    /// <inheritdoc />
    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<TPSetMsg<T>>(input);
        this.addSet = json.addSet;
        this.removeSet = json.removeSet;
    }

    /// <inheritdoc />
    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}


/// <summary>
/// Two Phase Set. Semantics follow that an element cannot be added to set after its removal.
/// </summary>
/// <typeparam name="T">Type the <c>TPSet{T}</c> shall hold.</typeparam>
[ReplicatedType("TPSet")]
public class TPSet<T> : CRDT, ICollection<T>
{
    /// <summary>
    /// Set holds elements which have been added to the <c>TPSet{T}</c>.
    /// </summary>
    private HashSet<T> addSet;

    /// <summary>
    /// Set holds elements which have been removed from the <c>TPSet{T}</c>.
    /// </summary>
    private HashSet<T> removeSet;

    /// <summary>
    /// Count of elements in the <c>TPSet{T}</c>.
    /// </summary>
    public int Count
    {
        get
        {
            return this.addSet.Count - this.removeSet.Count;
        }
    }

    /// <summary>
    /// Get a value indicating whether the <c>TPSet{T}</c> is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    public TPSet()
    {
        this.addSet = new HashSet<T>();
        this.removeSet = new HashSet<T>();
    }

    /// <summary>
    /// Add element <c>item</c> to the <c>TPSet{T}</c>.
    /// </summary>
    /// <param name="item">Element to add.</param>
    [OperationType(OpType.Update)]
    public virtual void Add(T item)
    {
        this.addSet.Add(item);
    }

    /// <summary>
    /// Remove element <c>item</c> from the <c>TPSet{T}</c>.
    /// </summary>
    /// <param name="item">Element to remove.</param>
    /// <returns></returns>
    [OperationType(OpType.Update)]
    public virtual bool Remove(T item)
    {
        if (this.Contains(item))
        {
            this.removeSet.Add(item);
            return true;
        }
        return false;

    }


    /// <summary>
    /// Queries all elements in the <c>TPSet{T}</c>.
    /// </summary>
    /// <returns><c>List</c> of elements in the <c>TPSet{T}</c>.</returns>
    public List<T> LookupAll()
    {
        return this.addSet.Except(this.removeSet).ToList();
    }


    /// <summary>
    /// Checks if <c>obj</c> is a <c>TPSet{T}</c> which contains the same set of elements.
    /// </summary>
    /// <param name="obj">Object to check against equality.</param>
    /// <returns><c>true</c> if <c>obj</c> is equal to <c>this</c>, <c>false</c> otherwise.</returns>
    public override bool Equals(object obj)
    {
        if (obj is TPSet<T>)
        {
            var other = (TPSet<T>) obj;
            return this.LookupAll().SequenceEqual(other.LookupAll());
        }

        return false;

    }

    /// <summary>
    /// Do not call, invalid operation. Cannot clear a <c>TPSet{T}</c>.
    /// </summary>
    public void Clear()
    {
        throw new InvalidOperationException("Cannot clear a TPSet");
    }

    /// <summary>
    /// Checks if <c>TPSet{T}</c> contains element.
    /// </summary>
    /// <param name="item">Element to check if contained in <c>TPSet{T}</c>.</param>
    /// <returns><c>true</c> if the element is in the <c>TPSet{T}</c>, <c>false</c> otherwise.</returns>
    public bool Contains(T item)
    {
        return this.addSet.Contains(item) && !this.removeSet.Contains(item);
    }

    /// <summary>
    /// Copies all the elements of the current <c>TPSet{T}</c> to the specified one-dimensional array.
    /// </summary>
    /// <param name="array">Target array to copy to.</param>
    /// <param name="arrayIndex">Index of <c>array</c> at which to begin copying.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Merges state from <c>received</c> to current <c>TPSet{T}</c>.
    /// </summary>
    /// <param name="received">State to merge into current <c>TPSet{T}</c>.</param>
    public void Merge(TPSetMsg<T> received)
    {
        this.addSet.UnionWith(received.addSet);
        this.removeSet.UnionWith(received.removeSet);
    }

    /// <inheritdoc />
    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        TPSetMsg<T> received = (TPSetMsg<T>) ReceivedUpdate;
        this.Merge(received);
    }

    /// <inheritdoc />
    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        TPSetMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    /// <inheritdoc />
    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new TPSetMsg<T>(this.addSet, this.removeSet);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>An <c>IEnumerator</c> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Serves as the hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return this.addSet.GetHashCode() ^ this.removeSet.GetHashCode();
    }
}
