using System;
using System.Collections.Generic;

namespace TeaDriven.Graphology.Traversal
{
    public interface IGetObjectGraph
    {
        GraphNode For(object currentObject, Type referenceType, string referenceName, IEnumerable<object> graphPath);
    }
}