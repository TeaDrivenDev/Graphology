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

            var createGraphologist = new CreateGraphologist()
                                     {
                                         //ExclusionRules = new MinimalExclusionRulesSet()
                                     };

            var graphologist = createGraphologist.Now();

            // Act
            var graph = graphologist.Graph(fixture);

            // Assert
        }
    }
}