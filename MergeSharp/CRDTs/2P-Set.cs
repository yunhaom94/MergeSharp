using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MergeSharp;


[TypeAntiEntropyProtocol(typeof(TPSet<>))]
public class TPSetMsg<T> : PropagationMessage
{
    [JsonInclude]
    public HashSet<T> addSet;
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


    public override void Decode(byte[] input)
    {
        var json = JsonSerializer.Deserialize<TPSetMsg<T>>(input);
        this.addSet = json.addSet;
        this.removeSet = json.removeSet;
    }

    public override byte[] Encode()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }
}


[ReplicatedType("TPSet")]
public class TPSet<T> : CRDT, ICollection<T>
{

    private HashSet<T> addSet;
    private HashSet<T> removeSet;

    public int Count
    {
        get
        {
            return this.addSet.Count - this.removeSet.Count;
        }
    }

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

    [OperationType(OpType.Update)]
    public virtual void Add(T item)
    {
        this.addSet.Add(item);
    }

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



    public List<T> LookupAll()
    {
        return this.addSet.Except(this.removeSet).ToList();
    }


    public override bool Equals(object obj)
    {
        if (obj is TPSet<T>)
        {
            var other = (TPSet<T>)obj;
            foreach (var item in this.LookupAll())
            {
                if (!other.Contains(item))
                {
                    return false;
                }
            }
        }
        return false;
        
    }

    public void Clear()
    {
        throw new InvalidOperationException("Cannot clear a TPSet");
    }

    public bool Contains(T item)
    {
        return this.addSet.Contains(item) && !this.removeSet.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }




    public void Merge(TPSetMsg<T> received)
    {
        this.addSet.UnionWith(received.addSet);
        this.removeSet.UnionWith(received.removeSet);
    }

    public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)
    {
        TPSetMsg<T> recieved = (TPSetMsg<T>)ReceivedUpdate;
        this.Merge(recieved);
    }

    public override PropagationMessage DecodePropagationMessage(byte[] input)
    {
        TPSetMsg<T> msg = new();
        msg.Decode(input);
        return msg;
    }

    public override PropagationMessage GetLastSynchronizedUpdate()
    {
        return new TPSetMsg<T>(this.addSet, this.removeSet);
    }

    
    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        return this.addSet.GetHashCode() ^ this.removeSet.GetHashCode();
    }
}