using TeaDriven.Graphology.Traversal;
using TeaDriven.Graphology.Visualization;

namespace TeaDriven.Graphology
{
    public class DefaultGraphologistComponents : GraphologistComponents
    {
        #region Constructors

        public DefaultGraphologistComponents()
            : base(new DefaultGraphTraversal(), new DefaultGraphVisualization()) { }

        public DefaultGraphologistComponents(TypeExclusions typeExclusions)
            : base(new DefaultGraphTraversal(typeExclusions), new DefaultGraphVisualization()) { }

        #endregion Constructors
    }
}