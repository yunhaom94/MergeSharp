using System;
using System.Collections.Generic;

namespace MergeSharp
{

    // a protfolio of types
    public enum ReplicatedDataTypes
    {
        PNCounter,

    }


    public abstract class PropagationMessage
    {
        public abstract byte[] Encode();

        public abstract void Decode(byte[] input);

        
    }

    public abstract class CRDT
    {

        public ReplicationManager manager;
        public Guid uid;

        protected void HasSideEffect()
        {   
            if (manager is not null)
                manager.NewUpdateToSync(this);
        }

        public abstract PropagationMessage GetLastSynchroizeUpdate();

        public abstract void ApplySynchronizedUpdate(PropagationMessage RecievedUpdate);

        public abstract PropagationMessage DecodePropagationMessage(byte[] input);
        


    }



}