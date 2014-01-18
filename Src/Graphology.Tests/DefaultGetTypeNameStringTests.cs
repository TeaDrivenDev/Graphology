using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;
using System;
using TeaDriven.Graphology.Tests.Conventions;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests
{
    public class DefaultGetTypeNameStringTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(DefaultGetTypeNameString).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetTypeNameString(DefaultGetTypeNameString sut)
        {
            Assert.IsAssignableFrom<IGetTypeNameString>(sut);
        }

        [Theory, AutoNSubstituteData]
        public void For_TypeHasNullGuard(DefaultGetTypeNameString sut)
        {
            // Arrange
            string @out;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.For(null, out @out));
        }

        [Theory, AutoNSubstituteData]
        public void For_ReturnsTrueAndTypeName(DefaultGetTypeNameString sut)
        {
            // Arrange
            var type = typeof(AutoNSubstituteDataAttribute);

            var expectedResult = true;
            var expectedTypeName = "AutoNSubstituteDataAttribute";

            // Act
            string resultTypeName;
            var result = sut.For(type, out resultTypeName);

            // Assert
            Assert.True(result);

            Assert.Equal(expectedTypeName, resultTypeName);
        }
    }
}