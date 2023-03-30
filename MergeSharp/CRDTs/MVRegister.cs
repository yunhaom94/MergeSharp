using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;
/// <summary>
/// Propagation Message for the <c>MVRegisterMsg{T}</c> class.
/// </summary>
/// <typeparam name="T">The type stored by the MV-Register.</typeparam>
[TypeAntiEntropyProtocol(typeof(MVRegister<>))]
public class MVRegisterMsg<T> : PropagationMessage
{
    /// <summary>
    /// Represent's the <c>MVRegister{T}</c>'s value(s).
    /// </summary>
    [JsonInclude]
    public HashSet<T> register { get; private set; }

    /// <summary>
    /// Represent's the <c>MVRegister{T}</c>'s vector clock.
    /// </summary>
    [JsonInclude]
    public Dictionary<Guid, int> vectorClock { get; private set; }

    public MVRegisterMsg()
    {
    }

    public MVRegisterMsg(HashSet<T> register, Dictionary<Guid, int> vectorClock)
    {
        this.register = register;
        this.vectorClock = vectorClock;
    }

    /// <inheritdoc />
    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<MVRegisterMsg<T>>(input);
        this.register = json.register;
        this.vectorClock = json.vectorClock;

    }

    /// <inheritdoc />
    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}

/// <summary>
/// Class <c>MVRegister{T}</c> models a Multi-Value Register.
/// </summary>
/// <typeparam name="T">The type stored by the MV-Register.</typeparam>
[ReplicatedType("MVRegister")]
public class MVRegister<T> : CRDT, IEnumerable<T>
{
    /// <summary>
    /// Represents the <c>MVRegister{T}</c>'s identification.
    /// </summary>
    private readonly Guid _mvr_id;

    /// <summary>
    /// Represents the <c>MVRegister{T}</c>'s value(s).
    /// </summary>
    private HashSet<T> _register;

    /// <summary>
    /// Dictionary(Guid,int) which holds the vector clocks of other MV-Register's.
    /// </summary>
    private readonly Dictionary<Guid, int> _vectorClock;

    /// <summary>
    /// Enum for the results of vector clock comparisons.
    /// </summary>
    private enum ComparisonResults
    {
        /// <summary>
        /// Indicates the register to be overwritten.
        /// </summary>
        Overwrite,
        /// <summary>
        /// Indicates the registers to be merged.
        /// </summary>
        Merge,
        /// <summary>
        /// Indicates the register to not be updated.
        /// </summary>
        NoUpdate
    }

    public MVRegister()
    {
        this._mvr_id = Guid.NewGuid();
        this._register = new HashSet<T>();
        this._vectorClock = new Dictionary<Guid, int>
        {
            { this._mvr_id, 0 }
        };
    }

    /// <summary>
    /// Updates the value of the register to the given item value. 
    /// </summary>
    /// <param name="item">the new value of the register.</param>
    [OperationType(OpType.Update)]
    public virtual void Write(T item)
    {
        this._register.Clear();
        bool _ = this._register.Add(item);
        this._vectorClock[this._mvr_id]++;
    }

    /// <summary>
    /// Updates the register based on the <c>MVRegisterMsg{T}</c> received.
    /// </summary>
    /// <param name="received"><c>MVRegisterMsg{T}</c> used to determine how the register will be updated.</param>
    private void Update(MVRegisterMsg<T> received)
    {
        ComparisonResults comparisonResult = this.CompareVectorClocks(received.vectorClock);

        this.UpdateMVRegWith(comparisonResult, received.register);
    }

    /// <summary>
    /// Uses the ComparisonResults enum to determine if the register with be overwritten or merged with the received register.
    /// </summary>
    /// <param name="comparisonResult">ComparisonResults enum which indicates whether the register will be overwritten or merged.</param>
    /// <param name="receivedRegister">Register that will be used for overwriting or merging.</param>
    private void UpdateMVRegWith(ComparisonResults comparisonResult, HashSet<T> receivedRegister)
    {
        if (comparisonResult == ComparisonResults.Overwrite)
        {
            this.OverwriteRegister(receivedRegister);
        }
        else if (comparisonResult == ComparisonResults.Merge)
        {
            this.MergeRegisters(receivedRegister);
        }
    }

    /// <summary>
    /// Overwrites the register the received register.
    /// </summary>
    /// <param name="receivedRegister">Register that will be used to overwrite the register.</param>
    private void OverwriteRegister(HashSet<T> receivedRegister)
    {
        this._register = receivedRegister;
    }

    /// <summary>
    /// Merges the register with the received register.
    /// </summary>
    /// <param name="receivedRegister">Register that will be merged with.</param>
    private void MergeRegisters(HashSet<T> receivedRegister)
    {
        this._register.UnionWith(receivedRegister);
    }

    /// <summary>
    /// Compares the register's vector clock with the vector clock in the received <c>MVRegisterMsg{T}</c>
    /// to determine what update needs to be made the register.
    /// </summary>
    /// <param name="receivedVectorClock">Vector clock that will be used to determine the action made to the register</param>
    /// <returns>A ComparisonResults enum that indicates the action made to the register.</returns>
    private ComparisonResults CompareVectorClocks(Dictionary<Guid, int> receivedVectorClock)
    {
        bool IsNewEntryAdded = false;
        int numOfEntriesUpdated = 0;

        foreach (KeyValuePair<Guid, int> entry in receivedVectorClock)
        {
            if (this._vectorClock.ContainsKey(entry.Key))
            {
                if (this._vectorClock[entry.Key] <= receivedVectorClock[entry.Key])
                {
                    this._vectorClock[entry.Key] = receivedVectorClock[entry.Key];
                    numOfEntriesUpdated++;
                }
            }
            else
            {
                this._vectorClock[entry.Key] = entry.Value;
                IsNewEntryAdded = true;
            }
        }

        if (IsNewEntryAdded)
        {
            return ComparisonResults.Merge;
        }
        else if (numOfEntriesUpdated != this._vectorClock.Count)
        {
            return ComparisonResults.Merge;
        }
        else if (numOfEntriesUpdated == this._vectorClock.Count)
        {
            return ComparisonResults.Overwrite;
        }
        else
        {
            return ComparisonResults.NoUpdate;
        }
    }

    /// <inheritdoc />
    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        if (ReceivedUpdate is MVRegisterMsg<T> received)
        {
            this.Update(received);
        }
        else
        {
            throw new NotSupportedException($"ReceivedUpdate does not support type of {ReceivedUpdate.GetType()}");
        }
    }

    /// <inheritdoc />
    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        MVRegisterMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    /// <inheritdoc />
    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new MVRegisterMsg<T>(this._register, this._vectorClock);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>) this._register).GetEnumerator();

    /// <summary>
    /// Exposes an enumerator, which supports a simple iteration over a non-generic collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) this._register).GetEnumerator();
}
