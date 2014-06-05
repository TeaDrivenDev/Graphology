using System;
using TeaDriven.Graphology.Traversal;
using TeaDriven.Graphology.Visualization;

namespace TeaDriven.Graphology
{
    public class Graphologist : IGraphologist
    {
        #region Dependencies

        private readonly IGraphTraversal _traversal;
        private readonly IGraphVisualization _visualization;

        #endregion Dependencies

        #region Constructors

        public Graphologist()
            : this(new DefaultGraphologistComponents()) { }

        public Graphologist(TypeExclusions typeExclusions)
            : this(new DefaultGraphologistComponents(typeExclusions)) { }

        public Graphologist(IGraphologistComponents components)
            : this(components.GraphTraversal, components.GraphVisualization) { }

        public Graphologist(IGraphTraversal traversal, IGraphVisualization visualization)
        {
            if (traversal == null) throw new ArgumentNullException("traversal");
            if (visualization == null) throw new ArgumentNullException("visualization");

            this._traversal = traversal;
            this._visualization = visualization;
        }

        #endregion Constructors

        #region IGraphologist Members

        public string Graph(object targetObject)
        {
            if (targetObject == null) throw new ArgumentNullException("targetObject");

            string graph = this._visualization.Draw(this._traversal.Traverse(targetObject));

            return graph;
        }

        #endregion IGraphologist Members
    }
}