using Ploeh.AutoFixture;
using System.Collections.Generic;
using Xunit;

namespace TeaDriven.Graphology.AutoFixture.Tests
{
    public class Class1
    {
        [Fact]
        public void A()
        {
            // Arrange
            var fixture = new Fixture();

            var createGraphologist = new CreateGraphologist();

            var graphologist = createGraphologist.Now();

            // Act
            var graph = graphologist.Graph(fixture);

            // Assert
        }

        [Fact]
        public void B()
        {
            // Arrange
            var sut = new DefaultGetObjectGraphRepresentation(new MinimalExclusionRulesSet());
            var graphVisualizer = new GraphVisualizer();

            var fixture = new Fixture();

            // Act
            var graph = sut.For(fixture, typeof(IFixture), "root", new List<object>());
            var graphString = graphVisualizer.Draw(graph);

            // Assert
        }
    }
}