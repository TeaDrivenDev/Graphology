using System;
using System.Collections.Generic;

namespace TeaDriven.Graphology.Visualization
{
    public class RecurrenceMarkingTextGraphVisualization : IGraphVisualization
    {
        #region Dependencies

        private readonly IGetNodeString _getNodeString;
        private readonly IGetHashCodeMappings _getHashCodeMappings;
        private readonly IGetRecurringObjectHashes _getRecurringObjectHashes;

        #endregion Dependencies

        #region Constructors

        public RecurrenceMarkingTextGraphVisualization(IGetNodeString getNodeString,
                                                       IGetHashCodeMappings getHashCodeMappings,
                                                       IGetRecurringObjectHashes getRecurringObjectHashes)
        {
            if (getNodeString == null) throw new ArgumentNullException("getNodeString");
            if (getHashCodeMappings == null) throw new ArgumentNullException("getHashCodeMappings");
            if (getRecurringObjectHashes == null) throw new ArgumentNullException("getRecurringObjectHashes");

            this._getNodeString = getNodeString;
            this._getHashCodeMappings = getHashCodeMappings;
            this._getRecurringObjectHashes = getRecurringObjectHashes;
        }

        #endregion Constructors

        #region IGraphVisualization Members

        public string Draw(GraphNode graphNode)
        {
            IEnumerable<int> recurringObjectHashes = this._getRecurringObjectHashes.From(graphNode);
            IDictionary<int, string> hashCodeMappings = this._getHashCodeMappings.For(recurringObjectHashes);

            return this.Draw(graphNode, 0, hashCodeMappings);
        }

        #endregion IGraphVisualization Members

        #region Internal methods

        private string Draw(GraphNode graphNode, int depth, IDictionary<int, string> hashCodeMappings)
        {
            string hashSuffix = (hashCodeMappings.ContainsKey(graphNode.ObjectHashCode)
                                     ? string.Format("({0}) ", hashCodeMappings[graphNode.ObjectHashCode])
                                     : "");

            string graph = string.Format(this._getNodeString.For(graphNode, depth), hashSuffix) + Environment.NewLine;

            depth++;

            foreach (GraphNode subGraphNode in graphNode.SubGraph)
            {
                graph += this.Draw(subGraphNode, depth, hashCodeMappings);
            }

            return graph;
        }

        #endregion Internal methods
    }
}