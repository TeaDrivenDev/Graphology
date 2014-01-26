using System;

namespace TeaDriven.Graphology.Traversal
{
    public interface ITypeExclusion
    {
        bool AppliesTo(Type type);
    }
}