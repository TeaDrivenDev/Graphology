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
            var fixture = new Fixture();

            var createGraphologist = new CreateNewGraphologist();
            var graphologist = createGraphologist.Now();

            // Act
            var graph = graphologist.Graph(fixture);

            // Assert
        }
    }
}