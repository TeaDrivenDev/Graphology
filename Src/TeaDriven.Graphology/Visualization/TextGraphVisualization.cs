using System;

namespace TeaDriven.Graphology.Visualization
{
    public class TextGraphVisualization : IGraphVisualization
    {
        #region Dependencies

        private readonly IGetNodeString _getNodeString;

        #endregion Dependencies

        #region Constructors

        public TextGraphVisualization(IGetNodeString getNodeString)
        {
            if (getNodeString == null) throw new ArgumentNullException("getNodeString");

            this._getNodeString = getNodeString;
        }

        #endregion Constructors

        #region IGraphVisualization Members

        public string Draw(GraphNode graphNode)
        {
            return this.Draw(graphNode, 0);
        }

        #endregion IGraphVisualization Members

        #region Internal methods

        private string Draw(GraphNode graphNode, int depth)
        {
            string graph = this._getNodeString.For(graphNode, depth) + Environment.NewLine;

            depth++;

            foreach (GraphNode subGraphNode in graphNode.SubGraph)
            {
                graph += this.Draw(subGraphNode, depth);
            }

            return graph;
        }

        #endregion Internal methods
    }
}