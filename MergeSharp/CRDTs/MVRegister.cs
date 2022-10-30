using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;

[TypeAntiEntropyProtocol(typeof(MVRegister<>))]
public class MVRegisterMsg<T> : PropagationMessage
{
    [JsonInclude]
    public HashSet<T> register { get; private set; }
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
    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<MVRegisterMsg<T>>(input);
        this.register = json.register;
        this.vectorClock = json.vectorClock;

    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}


[ReplicatedType("MVRegister")]
public class MVRegister<T> : CRDT, IEnumerable<T>
{
    private readonly Guid _mvr_id;
    private HashSet<T> _register;
    private readonly Dictionary<Guid, int> _vectorClock;

    private enum ComparisonResults
    {
        Overwrite,
        Merge,
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

    [OperationType(OpType.Update)]
    public void Write(T item)
    {
        this._register.Clear();
        bool _ = this._register.Add(item);
        this._vectorClock[this._mvr_id]++;
    }

    private void Update(MVRegisterMsg<T> received)
    {
        ComparisonResults comparisonResult = this.CompareVectorClocks(received.vectorClock);

        this.UpdateMVRegWith(comparisonResult, received.register);
    }

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

    private void OverwriteRegister(HashSet<T> receivedRegister)
    {
        this._register = receivedRegister;
    }

    private void MergeRegisters(HashSet<T> receivedRegister)
    {
        this._register.UnionWith(receivedRegister);
    }

    /// <summary>
    /// Compare this vector clock with the vector clock in the received MVRegisterMsg<T>
    /// to determine what update needs to be made this vector clock.
    /// </summary>
    /// <returns>Enum ComparisonResults that represents the result of the comparison</returns>    
    private ComparisonResults CompareVectorClocks(Dictionary<Guid, int> receivedVectorClock)
    {
        bool IsNewEntryAdded = false;
        // numOfEntries will be used to see if all existing entries were updated or not
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

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        MVRegisterMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new MVRegisterMsg<T>(this._register, this._vectorClock);
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>) this._register).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) this._register).GetEnumerator();
}