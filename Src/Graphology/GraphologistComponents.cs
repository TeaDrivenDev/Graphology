using System;
using TeaDriven.Graphology.Traversal;
using TeaDriven.Graphology.Visualization;

namespace TeaDriven.Graphology
{
    #region Facade objects

    public class GraphologistComponents : IGraphologistComponents
    {
        #region Dependencies

        private readonly IGraphTraversal _graphTraversal;
        private readonly IGraphVisualization _graphVisualization;

        #endregion Dependencies

        #region Constructors

        public GraphologistComponents(IGraphTraversal graphTraversal, IGraphVisualization graphVisualization)
        {
            if (graphTraversal == null) throw new ArgumentNullException("graphTraversal");
            if (graphVisualization == null) throw new ArgumentNullException("graphVisualization");

            this._graphTraversal = graphTraversal;
            this._graphVisualization = graphVisualization;
        }

        #endregion Constructors

        #region IGraphologistComponents Members

        public IGraphTraversal GraphTraversal
        {
            get { return this._graphTraversal; }
        }

        public IGraphVisualization GraphVisualization
        {
            get { return this._graphVisualization; }
        }

        #endregion IGraphologistComponents Members
    }

    #endregion Facade objects

    #region Graph visualization

    #endregion Graph visualization

    #region IGetObjectGraph

    #endregion IGetObjectGraph

    #region IGetSubGraph

    #endregion IGetSubGraph

    #region Exclusion Rules

    #endregion Exclusion Rules
}