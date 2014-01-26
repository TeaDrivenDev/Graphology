using System;
using System.Collections.Generic;
using System.Linq;

namespace TeaDriven.Graphology.Traversal
{
    public class DefaultGetObjectGraph : TypeExclusionsClientBase, IGetObjectGraph
    {
        #region Dependencies

        private readonly IGetSubGraph _getSubGraph;

        #endregion Dependencies

        #region Constructors

        public DefaultGetObjectGraph(IGetSubGraph getSubGraph, TypeExclusions typeExclusions)
            : base(typeExclusions)
        {
            if (getSubGraph == null) throw new ArgumentNullException("getSubGraph");

            this._getSubGraph = getSubGraph;
        }

        public DefaultGetObjectGraph(IGetSubGraph getSubGraph)
        {
            if (getSubGraph == null) throw new ArgumentNullException("getSubGraph");

            this._getSubGraph = getSubGraph;
        }

        #endregion Constructors

        #region IGetObjectGraph Members

        public GraphNode For(object currentObject, Type referenceType, string referenceName, IEnumerable<object> graphPath)
        {
            if (currentObject == null) throw new ArgumentNullException("currentObject");
            if (referenceType == null) throw new ArgumentNullException("referenceType");
            if (referenceName == null) throw new ArgumentNullException("referenceName");
            if (graphPath == null) throw new ArgumentNullException("graphPath");

            GraphNode node = new GraphNode()
                             {
                                 ReferenceType = referenceType,
                                 ObjectType = currentObject.GetType(),
                                 ReferenceName = referenceName,
                                 ObjectHashCode = currentObject.GetHashCode()
                             };

            if (graphPath.Contains(currentObject))
            {
                node.IsRecursionStart = true;
            }
            else if (!this.DoNotFollowType(currentObject.GetType()))
            {
                IList<GraphNode> subGraph = new List<GraphNode>();
                bool handled = this._getSubGraph.For(currentObject, graphPath.Concat(new List<object>() { currentObject }),
                                                     out subGraph);

                node.SubGraph = subGraph;
            }

            return node;
        }

        #endregion IGetObjectGraph Members
    }
}