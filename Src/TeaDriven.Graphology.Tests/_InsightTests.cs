﻿using TeaDriven.Graphology.Traversal;
using TeaDriven.Graphology.Visualization;
using Xunit;

namespace TeaDriven.Graphology.Tests
{
    public class _InsightTests
    {
        // Ignore in NCrunch; run in ReSharper/xUnit runner(?) to re-generate the object graph
        [Fact]
        public void WriteGraphologistGraph()
        {
            // Arrange
            var projectPath = @"..\TeaDriven.Graphology";
            var graphName = "Graphologist";

            var graphologist = new Graphologist(new DefaultGraphTraversal(new MinimalTypeExclusions()
                                                                          {
                                                                              Exclude = new ExactNamespaceTypeExclusion("System")
                                                                          }), new DefaultGraphVisualization());

            var target = graphologist;

            // Act
            graphologist.WriteGraph(target, projectPath, graphName);

            // Assert
        }

        // Ignore in NCrunch; run in ReSharper/xUnit runner(?) to re-generate the object graph
        [Fact]
        public void WriteDefaultGraphVisualizationGraph()
        {
            // Arrange
            var projectPath = @"..\TeaDriven.Graphology";
            var graphName = "DefaultGraphVisualization";

            var graphologist = new Graphologist(new MinimalTypeExclusions()
                                                {
                                                    Exclude = new ExactNamespaceTypeExclusion("System")
                                                });

            // This would be the evaluation of your composition root, like an IoC container resolution.
            var target = new DefaultGraphVisualization();

            // Act
            graphologist.WriteGraph(target, projectPath, graphName);

            // Assert
        }
    }
}