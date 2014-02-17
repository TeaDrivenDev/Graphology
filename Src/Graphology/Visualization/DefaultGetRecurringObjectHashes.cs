using System.Collections.Generic;
using System.Linq;

namespace TeaDriven.Graphology.Visualization
{
    public class DefaultGetRecurringObjectHashes : IGetRecurringObjectHashes
    {
        #region IGetRecurringObjectHashes Members

        public IEnumerable<int> From(GraphNode graph)
        {
            IEnumerable<GraphNode> nodes = graph.Traverse().Where(n => !n.IsRecursionStart);
            IEnumerable<GraphNode> recurringNodes = nodes.GroupBy(n => n.ObjectHashCode).Where(g => g.Count() > 1).Select(g => g.First());

            //int removed = 0;

            //do
            //{
            //    IEnumerable<GraphNode> toRemove = recurringNodes.Where(n => recurringNodes.Any(n2 => n2.SubGraph.Contains(n))).ToList();

            //    removed = toRemove.Count();

            //    recurringNodes = recurringNodes.Except(toRemove);
            //}
            //while (removed > 0);

            IEnumerable<int> hashes = recurringNodes.Select(n => n.ObjectHashCode);

            return hashes;
        }

        #endregion IGetRecurringObjectHashes Members
    }
}