using Xunit;
using MergeSharp;
using System;
using System.Collections.Generic;

namespace MergeSharp.Tests;

public class MVRegisterTests
{
    [Fact]
    public void TestMVRegisterSingle()
    {
        MVRegister<string> register = new();

        register.Write("a");
        register.Write("b");
        register.Write("c");
        Assert.Equal(new List<string> {"c"}, register); 
    }

    [Fact]
    public void TestMVRegisterConcurrentMerge()
    {
        MVRegister<string> reg1 = new();
        reg1.Write("a");
        reg1.Write("b");

        MVRegister<string> reg2 = new();
        reg2.Write("c");
        reg2.Write("d");

        reg1.ApplySynchronizedUpdate((MVRegisterMsg<string>)reg2.GetLastSynchronizedUpdate());

        Assert.Equal(new List<string> { "b", "d" }, reg1);
    
        reg2.ApplySynchronizedUpdate((MVRegisterMsg<string>)reg1.GetLastSynchronizedUpdate());

        Assert.Equal(new List<string> { "d", "b" }, reg2);
    }

    [Fact]
    public void TestMVRegisterConcurrentMerge2()
    {
        MVRegister<int> reg1 = new();
        MVRegister<int> reg2 = new();
        MVRegister<int> reg3 = new();
        MVRegister<int> reg4 = new();

        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg2.GetLastSynchronizedUpdate());
        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg3.GetLastSynchronizedUpdate());
        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg4.GetLastSynchronizedUpdate());

        reg4.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg1.GetLastSynchronizedUpdate());
        reg4.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg2.GetLastSynchronizedUpdate());
        reg4.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg3.GetLastSynchronizedUpdate());

        reg4.Write(4);        

        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg4.GetLastSynchronizedUpdate());
        
        Assert.Equal(new List<int> { 4 }, reg1);
    }

    [Fact]
    public void TestMVRegisterConcurrentMerge3()
    {
        MVRegister<int> reg1 = new();
        MVRegister<int> reg2 = new();
        MVRegister<int> reg3 = new();
        MVRegister<int> reg4 = new();

        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg2.GetLastSynchronizedUpdate());
        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg3.GetLastSynchronizedUpdate());
        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg4.GetLastSynchronizedUpdate());

        reg4.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg1.GetLastSynchronizedUpdate());
        reg4.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg2.GetLastSynchronizedUpdate());
        reg4.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg3.GetLastSynchronizedUpdate());

        reg1.Write(1);
        reg4.Write(4);        

        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg4.GetLastSynchronizedUpdate());
        
        Assert.Equal(new List<int> { 1, 4 }, reg1);
    }

    [Fact]
    public void TestMVRegisterOverwrite()
    {
        MVRegister<string> reg1 = new();
        reg1.Write("a");
        reg1.Write("b");

        MVRegister<string> reg2 = new();
    
        reg2.ApplySynchronizedUpdate((MVRegisterMsg<string>)reg1.GetLastSynchronizedUpdate());

        Assert.Equal(new List<string> { "b" }, reg2);

        reg2.Write("c");

        Assert.Equal(new List<string> { "c" }, reg2);
        
        reg1.ApplySynchronizedUpdate((MVRegisterMsg<string>)reg2.GetLastSynchronizedUpdate());
    }

    [Fact]
    public void TestRegisterIsEnumerable()
    {
        MVRegister<int> reg1 = new();
        MVRegister<int> reg2 = new();
        MVRegister<int> reg3 = new();
        MVRegister<int> reg4 = new();

        reg1.Write(1);
        reg2.Write(2);
        reg3.Write(3);
        reg4.Write(4);

        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg2.GetLastSynchronizedUpdate());
        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg3.GetLastSynchronizedUpdate());
        reg1.ApplySynchronizedUpdate((MVRegisterMsg<int>)reg4.GetLastSynchronizedUpdate());

        Assert.Equal(new List<int> { 1, 2, 3, 4 }, reg1);
    }

}