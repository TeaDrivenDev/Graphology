using System;
using System.Collections.Generic;

namespace TeaDriven.Graphology.Traversal
{
    public class GraphTraversal : IGraphTraversal
    {
        #region Dependencies

        private readonly IGetObjectGraph _getObjectGraph;

        #endregion Dependencies

        #region Constructors

        public GraphTraversal(IGetObjectGraph getObjectGraph)
        {
            if (getObjectGraph == null) throw new ArgumentNullException("getObjectGraph");

            this._getObjectGraph = getObjectGraph;
        }

        #endregion Constructors

        #region IGraphTraversal Members

        public GraphNode Traverse(object targetObject)
        {
            if (targetObject == null) throw new ArgumentNullException("targetObject");

            GraphNode graph = this._getObjectGraph.For(targetObject, targetObject.GetType(), "root", new List<object>());

            return graph;
        }

        #endregion IGraphTraversal Members
    }
}