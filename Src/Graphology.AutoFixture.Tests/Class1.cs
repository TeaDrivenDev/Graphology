using Ploeh.AutoFixture;
using Xunit;

namespace TeaDriven.Graphology.AutoFixture.Tests
{
    public class Class1
    {
        [Fact]
        public void CanCreateGraph()
        {
            // Arrange

            var fixture = new Fixture();

            var graphologist = new Graphologist(new MinimalTypeExclusions()
                                                {
                                                    Exclude = new ExactNamespaceTypeExclusion("System")
                                                });

            // Act
            var graph = graphologist.Graph(fixture);
            // Assert
        }

        // Ignore this test in NCrunch; run in ReSharper to regenerate the object graph
        [Fact]
        public void WriteGraph()
        {
            // Arrange
            var projectPath = @"..\Graphology.AutoFixture";
            var graphName = "Fixture";

            var fixture = new Fixture();

            var graphologist = new Graphologist(new MinimalTypeExclusions()
            {
                Exclude = new ExactNamespaceTypeExclusion("System")
            });

            // Act
            var graph = graphologist.Graph(fixture);
            graphologist.WriteGraph(fixture, projectPath, graphName);

            // Assert
        }
    }
}