using Xunit;
using System;

using MergeSharp;
using MergeSharp.TCPConnectionManager;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MergeSharp.Tests;

// xunits classes are automatically run in parallel
[Collection("collection1")]
public class TCPConnectionManagerTwoNodesTests : IDisposable
{

    ReplicationManager rm0, rm1;

    public TCPConnectionManagerTwoNodesTests()
    {
        List<string> nodeslist = new List<string>(new string[] { "127.0.0.1:8000", "127.0.0.1:8001" });

        ConnectionManager cm0 = new ConnectionManager("127.0.0.1", "8000", nodeslist);
        ConnectionManager cm1 = new ConnectionManager("127.0.0.1", "8001", nodeslist);

        this.rm0 = new ReplicationManager(cm0);
        this.rm1 = new ReplicationManager(cm1);

        // register types
        this.rm0.RegisterType<PNCounter>();
        this.rm1.RegisterType<PNCounter>();

    }


    public void Dispose()
    {
        Thread.Sleep(1000);
        rm0.Dispose();
        rm1.Dispose();
        Thread.Sleep(1000);
    }

    [Fact]
    public void TCPClusterPNCTest()
    {

        Guid uid = this.rm0.CreateCRDTInstance<PNCounter>(out PNCounter pnc1);

        pnc1.Increment(5);
        pnc1.Decrement(8);
        pnc1.Increment(10);
        pnc1.Decrement(3);

        // sleep
        Thread.Sleep(1000);

        var pnc1r = rm1.GetCRDT<PNCounter>(uid);   

        Assert.Equal(pnc1.Get(), pnc1r.Get());


    }

    [Fact]
    public void TCPClusterPNCTest2()
    {

        Guid uid = this.rm0.CreateCRDTInstance<PNCounter>(out PNCounter pnc1);

        Thread.Sleep(1000);

        pnc1.Increment(5);
        pnc1.Decrement(8);

        Thread.Sleep(1000);

        var pnc1r = rm1.GetCRDT<PNCounter>(uid);
        Assert.Equal(pnc1.Get(), pnc1r.Get());

        pnc1r.Increment(10);
        pnc1r.Decrement(3);

        Thread.Sleep(1000);


        Assert.Equal(pnc1.Get(), pnc1r.Get());
    }




}

[Collection("collection1")]
public class TCPConnectionManagerMultiNodesTests : IDisposable
{
    ReplicationManager rm0, rm1, rm2, rm3, rm4;

    public TCPConnectionManagerMultiNodesTests()
    {
        List<string> nodeslist = new List<string>(new string[]
        {
                "127.0.0.1:8000",
                "127.0.0.1:8001",
                "127.0.0.1:8002",
                "127.0.0.1:8003",
                "127.0.0.1:8004"
        });

        ConnectionManager cm0 = new ConnectionManager("127.0.0.1", "8000", nodeslist);
        ConnectionManager cm1 = new ConnectionManager("127.0.0.1", "8001", nodeslist);
        ConnectionManager cm2 = new ConnectionManager("127.0.0.1", "8002", nodeslist);
        ConnectionManager cm3 = new ConnectionManager("127.0.0.1", "8003", nodeslist);
        ConnectionManager cm4 = new ConnectionManager("127.0.0.1", "8004", nodeslist);


        this.rm0 = new ReplicationManager(cm0);
        this.rm1 = new ReplicationManager(cm1);
        this.rm2 = new ReplicationManager(cm2);
        this.rm3 = new ReplicationManager(cm3);
        this.rm4 = new ReplicationManager(cm4);

        // register PNCounter type on each replication manager
        this.rm0.RegisterType<PNCounter>();
        this.rm1.RegisterType<PNCounter>();
        this.rm2.RegisterType<PNCounter>();
        this.rm3.RegisterType<PNCounter>();
        this.rm4.RegisterType<PNCounter>();


    }

    [Fact]
    public void MNTCPClusterPNCTest()
    {
        Guid uid = this.rm0.CreateCRDTInstance<PNCounter>(out PNCounter pnc);

        Thread.Sleep(1000);

        // get the PNCounter from the other nodes
        var pnc1 = rm1.GetCRDT<PNCounter>(uid);
        var pnc2 = rm2.GetCRDT<PNCounter>(uid);
        var pnc3 = rm3.GetCRDT<PNCounter>(uid);
        var pnc4 = rm4.GetCRDT<PNCounter>(uid);

        // list of pncs
        List<PNCounter> pncs = new List<PNCounter>() { pnc, pnc1, pnc2, pnc3, pnc4 };


        // Increment each PNCounter by a random amount, repeat 5 times
        Random rnd = new Random();
        foreach (var p in pncs)
        {
            int amount = rnd.Next(1, 10);
            p.Increment(amount);            
        }

        // check if the same pncounters are replicated on all nodes
    

        // sleep for 1 second
        Thread.Sleep(5000);

        Assert.Equal(pncs[0].Get(), pncs[1].Get());
        Assert.Equal(pncs[0].Get(), pncs[2].Get());
        Assert.Equal(pncs[0].Get(), pncs[3].Get());
        Assert.Equal(pncs[0].Get(), pncs[4].Get());

    }


    public void Dispose()
    {
        Thread.Sleep(1000);
    }
}