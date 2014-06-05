using System;
using System.Collections.Generic;
using System.Reflection;

namespace TeaDriven.Graphology.Traversal
{
    public interface IGetTypeFields
    {
        IEnumerable<FieldInfo> For(Type type);
    }
}