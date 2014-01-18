using System;
using NSubstitute;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;
using TeaDriven.Graphology.Tests.Conventions;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests
{
    public class LazyGetTypeNameStringTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(LazyGetTypeNameString).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetTypeNameString(LazyGetTypeNameString sut)
        {
            Assert.IsAssignableFrom<IGetTypeNameString>(sut);
        }

        [Theory, AutoNSubstituteData]
        public void For_TypeHasNullGuard(LazyGetTypeNameString sut)
        {
            // Arrange
            string @out;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.For(null, out @out));
        }

        [Theory]
        [InlineAutoNSubstituteData(true)]
        [InlineAutoNSubstituteData(false)]
        public void For_ReturnsResultsFromInnerInstance(bool expectedResult,
                                                        [Frozen] IGetTypeNameString innerInstance,
                                                        LazyGetTypeNameString sut,
                                                        Type type,
                                                        string expectedTypeName)
        {
            // Arrange
            var @out = Arg.Any<string>();

            innerInstance.For(type, out @out).Returns(x =>
                                                      {
                                                          x[1] = expectedTypeName;
                                                          return expectedResult;
                                                      });

            // Act
            string resultTypeName;
            var result = sut.For(type, out resultTypeName);

            // Assert
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedTypeName, resultTypeName);
        }
    }
}