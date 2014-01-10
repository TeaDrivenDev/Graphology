using Ploeh.AutoFixture;
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

            var graphologist = new Graphologist(new MinimalExclusionRulesSet());

            // Act
            var graph = graphologist.Graph(fixture);

            graphologist = new Graphologist(new MinimalExclusionRulesSet());

            graph = graphologist.Graph(fixture);

            // Assert
        }
    }
}