using System;


namespace MergeSharp
{   
    /// <summary>
    /// Any data structure that a ConnectionsManager uses to manage connections to
    /// other nodes/replicas in the system need to inherit from this class.
    /// </summary>
    public abstract class ConnectionEndpoint
    {

    }

    public class SyncMsgEventArgs : EventArgs
    {
        public NetworkProtocol msg { private set; get; } 
        public ReplicatedDataTypes type { private set; get; } 
        public SyncMsgEventArgs(NetworkProtocol msg) 
        {
            this.msg = msg;
        }
        
    }


    ///
    public delegate void ReceivedSyncEventHandler(object sender, SyncMsgEventArgs e);

    /// <summary>
    /// A concrete ConnectionManager class needs to implement this interface
    /// that handles the communication among nodes/replicas.
    /// </summary>
    public interface IConnectionManager : IDisposable
    {
        /// <summary>
        /// Call this delegate when a new synchronization message is received 
        /// so the ReplicationManager can perform the necessary synchronization on stored objects.
        /// </summary>
        public event EventHandler<SyncMsgEventArgs> ReplicationManagerSyncMsgHandlerEvent;

        /// <summary>
        /// Broadcasts a synchronization message to all replicas.
        /// </summary>
        /// <param name="msg">Sync message to be broadcasted</param>
        public void PropagateSyncMsg(NetworkProtocol msg);

        /// <summary>
        /// Starts ConnectionManager server.
        /// </summary>
        public void Start();

        /// <summary>
        /// Ends ConnectionManager server.
        /// </summary>
        public void Stop();


    }
}
