using Ploeh.AutoFixture.Idioms;
using TeaDriven.Graphology.Tests.Conventions;
using TeaDriven.Graphology.Visualization;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests.Visualization
{
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
}