
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MergeSharp;

internal class ProxyBuilder
{
    private AssemblyName aName;
    private AssemblyBuilder ab;
    private ModuleBuilder mb;


    public ProxyBuilder()
    {
        this.aName = new AssemblyName("CRDTClassProxyAssembly");
        this.ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
        this.mb = ab.DefineDynamicModule(aName.Name + ".dll");
        // TODO: maybe check AppDomain for hot swap of types
    }

    private MethodInfo[] GetMethodsToWrap(Type type)
    {
        var methods = type.GetMethods();

        // result list
        var result = new List<MethodInfo>();

        // for each method in methods, if it has TestStuffAttribute, print it
        foreach (var method in methods)
        {
            var op = MethodValidator.Validate(method);
            if (op == OpType.Update)
            {
                result.Add(method);
            }

        }
        return result.ToArray();
    }

    public Type BuildProxiedClass(Type type)
    {

        TypeBuilder tb = mb.DefineType(type.Name + "Proxy", TypeAttributes.Public | TypeAttributes.Class, type);

        foreach (var method in this.GetMethodsToWrap(type))
        {

            var methodBuilder = tb.DefineMethod(method.Name,
                                    MethodAttributes.Public | MethodAttributes.Virtual,
                                    method.ReturnType,
                                    method.GetParameters().Select(p => p.ParameterType).ToArray());
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            // load all of the orignal method arugments into the stack
            for (int i = 0; i < method.GetParameters().Length; i++)
            {
                il.Emit(OpCodes.Ldarg, i + 1);
            }
            il.Emit(OpCodes.Call, method);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, typeof(CRDT).GetMethod("HasSideEffect", BindingFlags.NonPublic | BindingFlags.Instance));
            il.Emit(OpCodes.Ret);
        }

        return tb.CreateType();

    }



}