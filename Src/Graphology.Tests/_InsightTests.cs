using NSubstitute;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;
using System.Collections.Generic;
using System.Linq;
using TeaDriven.Graphology.Tests.Conventions;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests
{
    public class _InsightTests
    {
        // Ignore in NCrunch; run in ReSharper/xUnit runner(?) to re-generate the object graph
        [Fact]
        public void WriteGraphologistGraph()
        {
            // Arrange
            var projectPath = @"..\Graphology";
            var graphName = "Graphologist";

            var graphologist = new Graphologist(new DefaultGraphTraversal(new MinimalTypeExclusions()
                                                                          {
                                                                              Exclude = new ExactNamespaceTypeExclusion("System")
                                                                          }), new DefaultGraphVisualization2());

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
            var projectPath = @"..\Graphology";
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

    public class DefaultGetNextLetterCodeTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(DefaultGetNextLetterCode).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetNextLetterCode(DefaultGetNextLetterCode sut)
        {
            Assert.IsAssignableFrom<IGetNextLetterCode>(sut);
        }

        [Theory]
        [InlineAutoNSubstituteData("A", "B")]
        [InlineAutoNSubstituteData("M", "N")]
        public void From_ReturnsCorrectNextLetterCode(string previous, string expectedResult, DefaultGetNextLetterCode sut)
        {
            // Arrange

            // Act
            var result = sut.From(previous);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }

    public class DefaultGetHashCodeMappingsTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(DefaultGetHashCodeMappings).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetHashCodeMappings(DefaultGetHashCodeMappings sut)
        {
            Assert.IsAssignableFrom<IGetHashCodeMappings>(sut);
        }

        [Theory, AutoNSubstituteData]
        public void For_ReturnsCorrectProgression([Frozen] IGetNextLetterCode getNextLetterCode, DefaultGetHashCodeMappings sut)
        {
            // Arrange
            getNextLetterCode.From(Arg.Any<string>()).Returns("B", "C", "D", "E");

            var input = new[] { 56, 343, 111, 53 };

            var expectedResult = new Dictionary<int, string>()
                                 {
                                     {56, "A"},
                                     {343, "B"},
                                     {111, "C"},
                                     {53, "D"}
                                 };

            // Act
            var result = sut.For(input);

            // Assert
            Assert.True(result.SequenceEqual(expectedResult));
        }
    }
}