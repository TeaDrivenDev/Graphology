using System;
using System.Collections.Generic;

namespace TeaDriven.Graphology.Traversal
{
    public class LazyGetObjectGraph : IGetObjectGraph
    {
        #region Relayed instance

        public IGetObjectGraph GetObjectGraph { get; set; }

        #endregion Relayed instance

        #region IGetObjectGraph Members

        public GraphNode For(object currentObject, Type referenceType, string referenceName, IEnumerable<object> graphPath)
        {
            GraphNode graph = this.GetObjectGraph.For(currentObject, referenceType, referenceName, graphPath);

            return graph;
        }

        #endregion IGetObjectGraph Members
    }
}