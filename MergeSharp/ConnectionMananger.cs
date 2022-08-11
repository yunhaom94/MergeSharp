using System;


namespace MergeSharp
{
    public abstract class ConnectionEndpoint
    {

    }

    public class SyncMsgEventArgs : EventArgs
    {
        public SyncProtocol msg { private set; get; } 
        public ReplicatedDataTypes type { private set; get; } 
        public SyncMsgEventArgs(SyncProtocol msg) 
        {
            this.msg = msg;
        }
        
    }



    public delegate void RecivedSyncEventHandler(object sender, SyncMsgEventArgs e);
    
    // This needs to be implemented for users wish to use CRDT
    // or user the default one
    public interface IConnectionManager : IDisposable
    {

        public event EventHandler<SyncMsgEventArgs> UpdateSyncRecievedHandleEvent;

        public void PropagateSyncMsg(SyncProtocol msg);

        public void Start();

        public void Stop();

        public int NumMembers();

        public int[] AllMemberIds();


        public int CurMemberPosition();




    }
}
