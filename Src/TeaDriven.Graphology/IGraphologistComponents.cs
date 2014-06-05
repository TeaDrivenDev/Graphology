using TeaDriven.Graphology.Traversal;
using TeaDriven.Graphology.Visualization;

namespace TeaDriven.Graphology
{
    public interface IGraphologistComponents
    {
        IGraphTraversal GraphTraversal { get; }

        IGraphVisualization GraphVisualization { get; }
    }
}