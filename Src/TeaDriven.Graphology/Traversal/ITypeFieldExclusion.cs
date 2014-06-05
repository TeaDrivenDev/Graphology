using System;
using System.Reflection;

namespace TeaDriven.Graphology.Traversal
{
    public interface ITypeFieldExclusion
    {
        bool AppliesTo(Type type, FieldInfo field);
    }
}