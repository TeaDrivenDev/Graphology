using NSubstitute;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;
using System.Collections.Generic;
using System.Linq;
using TeaDriven.Graphology.Tests.Conventions;
using TeaDriven.Graphology.Visualization;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests.Visualization
{
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