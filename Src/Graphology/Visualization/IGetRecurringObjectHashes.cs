using System.Collections.Generic;

namespace TeaDriven.Graphology.Visualization
{
    public interface IGetRecurringObjectHashes
    {
        IEnumerable<int> From(GraphNode graph);
    }
}