using System;
using System.Reflection;

namespace MergeSharp;

[System.AttributeUsage(System.AttributeTargets.Class)]
public class ReplicatedTypeAttribute : System.Attribute
{

    private string name;

    public ReplicatedTypeAttribute(string name)
    {
        this.name = name;
    }
}

[System.AttributeUsage(System.AttributeTargets.Class)]
public class TypeAntiEntropyProtocolAttribute : System.Attribute
{

    public Type type { get; private set; }

    public TypeAntiEntropyProtocolAttribute(Type type)
    {
        this.type = type;
    }
}


public enum OpType
{
    Update,
    Query,

}

// OperationTypeAttribute for CRDT methods
[System.AttributeUsage(System.AttributeTargets.Method)]
public class OperationTypeAttribute : System.Attribute
{

    public OpType opType;

    public OperationTypeAttribute(OpType opType)
    {
        this.opType = opType;
    }
}



internal static class MethodValidator
{

    public static OpType? Validate(MethodInfo method)
    {
        var attribute = method.GetCustomAttribute<OperationTypeAttribute>();

        if (attribute is not null)
        {
            if (!method.IsPublic)
            {
                throw new Exception("Method " + method.Name + " is not public while marked " + attribute.opType.ToString());
            }

            if (attribute.opType == OpType.Update)
            {
                // if method is not virtual
                if (!method.IsVirtual)
                {
                    throw new Exception("Method " + method.Name + " is not virtual while marked " + attribute.opType.ToString());
                }
            }

            return attribute.opType;
        }

        return null;
    }




}


