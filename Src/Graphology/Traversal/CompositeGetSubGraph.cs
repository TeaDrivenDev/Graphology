using System;
using System.Collections.Generic;

namespace TeaDriven.Graphology.Traversal
{
    public class CompositeGetSubGraph : IGetSubGraph
    {
        #region Dependencies

        private readonly IEnumerable<IGetSubGraph> _getSubGraphRepresentations;

        #endregion Dependencies

        #region Constructors

        public CompositeGetSubGraph(params IGetSubGraph[] getSubGraphRepresentations)
        {
            if (getSubGraphRepresentations == null) throw new ArgumentNullException("getSubGraphRepresentations");

            this._getSubGraphRepresentations = getSubGraphRepresentations;
        }

        public CompositeGetSubGraph(IEnumerable<IGetSubGraph> getSubGraphRepresentations)
        {
            if (getSubGraphRepresentations == null) throw new ArgumentNullException("getSubGraphRepresentations");

            this._getSubGraphRepresentations = getSubGraphRepresentations;
        }

        #endregion Constructors

        #region IGetSubGraph Members

        public bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph)
        {
            List<GraphNode> localSubGraph = new List<GraphNode>();

            bool handled = false;

            foreach (IGetSubGraph getSubGraph in this._getSubGraphRepresentations)
            {
                IList<GraphNode> subSubGraph;

                handled = getSubGraph.For(currentObject, graphPath, out subSubGraph);

                localSubGraph.AddRange(subSubGraph);
            }

            subGraph = localSubGraph;

            return handled;
        }

        #endregion IGetSubGraph Members
    }
}