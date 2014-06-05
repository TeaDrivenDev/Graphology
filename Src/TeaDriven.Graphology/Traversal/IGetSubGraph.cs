using System.Collections.Generic;

namespace TeaDriven.Graphology.Traversal
{
    public interface IGetSubGraph
    {
        bool For(object currentObject, IEnumerable<object> graphPath, out IList<GraphNode> subGraph);
    }
}