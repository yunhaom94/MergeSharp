using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MergeSharp
{

    // a protfolio of types
    public enum ReplicatedDataTypes
    {
        PNCounter,

    }

    /// <summary>
    /// The anti-entropy/synchronization message for a CRDTs.
    /// Each CRDT type must implement it.
    /// </summary>
    public abstract class PropagationMessage
    {
        /// <summary>
        /// Serialize the message to a byte array.
        /// </summary>
        /// <returns>Serialized message</returns>
        public abstract byte[] Encode();

        /// <summary>
        /// Deserialize the message from a byte array.
        /// </summary>
        /// <param name="input"></param>
        public abstract void Decode(byte[] input);

        
    }

    /// <summary>
    /// Base class for all CRDT types. See README for more details on implementing new CRDTs.
    /// </summary>
    public abstract class CRDT
    {   
        protected readonly ILogger logger;
        public ReplicationManager manager;
        public Guid uid;

        public CRDT(ILogger logger = null)
        {
            this.logger = logger ?? NullLogger.Instance;
        }

        protected void HasSideEffect()
        {   
            if (manager is not null)
                manager.NewUpdateToSync(this);
        }

        /// <summary>
        /// Returns the anti-entropy message for a recent side-effect update to this (source) CRDT.
        /// For example, in state-based CRDTs, this is the state itself; in op-based CRDTs, this is the prepare-update operation.
        /// </summary>
        /// <returns>The synchronization message</returns>
        public abstract PropagationMessage GetLastSynchronizedUpdate();

        /// <summary>
        /// Apply an received synchronization anti-entropy message to this (downstream) CRDT.
        /// In state-based CRDTs, this is the Merge() method; in op-based CRDTs, this is the effect-update operation.
        /// </summary>
        /// <param name="ReceivedUpdate">The received synchronization message</param>
        public abstract void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate);

        /// <summary>
        /// Deserialize a byte array into an Anti-entropy message for this CRDT and the message type it is using.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public abstract PropagationMessage DecodePropagationMessage(byte[] input);
        


    }



}