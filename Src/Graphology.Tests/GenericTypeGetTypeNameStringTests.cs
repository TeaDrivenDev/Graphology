using Ploeh.AutoFixture.Idioms;
using System;
using System.Collections.Generic;
using TeaDriven.Graphology.Tests.Conventions;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests
{
    public class GenericTypeGetTypeNameStringTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(GenericTypeGetTypeNameString).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetTypeNameString(GenericTypeGetTypeNameString sut)
        {
            Assert.IsAssignableFrom<IGetTypeNameString>(sut);
        }

        [Theory, AutoNSubstituteData]
        public void For_TypeHasNullGuard(GenericTypeGetTypeNameString sut)
        {
            // Arrange
            string @out;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.For(null, out @out));
        }

        [Theory, AutoNSubstituteData]
        public void For_NonGenericType_ReturnsFalse(GenericTypeGetTypeNameString sut)
        {
            // Arrange
            var type = typeof(AutoNSubstituteDataAttribute);

            // Act
            string @out;
            var result = sut.For(type, out @out);

            // Assert
            Assert.False(result);
        }

        [Theory, AutoNSubstituteData]
        public void For_GenericTypeOneParameter_ReturnsTrueAndTypeName(GenericTypeGetTypeNameString sut)
        {
            // Arrange
            var type = typeof(IList<AutoNSubstituteDataAttribute>);

            var expectedTypeName = "IList<AutoNSubstituteDataAttribute>";

            // Act
            string resultTypeName;
            var result = sut.For(type, out resultTypeName);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedTypeName, resultTypeName);
        }

        [Theory, AutoNSubstituteData]
        public void For_GenericTypeThreeParameters_ReturnsTrueAndTypeName(GenericTypeGetTypeNameString sut)
        {
            // Arrange
            var type = typeof(Tuple<AutoNSubstituteDataAttribute, Int64, InsufficientMemoryException>);

            var expectedTypeName = "Tuple<AutoNSubstituteDataAttribute, Int64, InsufficientMemoryException>";

            // Act
            string resultTypeName;
            var result = sut.For(type, out resultTypeName);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedTypeName, resultTypeName);
        }
    }
}