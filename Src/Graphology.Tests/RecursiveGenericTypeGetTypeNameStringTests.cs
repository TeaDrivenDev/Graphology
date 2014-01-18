using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;
using System;
using System.Collections.Generic;
using TeaDriven.Graphology.Tests.Conventions;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests
{
    public class RecursiveGenericTypeGetTypeNameStringTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(RecursiveGenericTypeGetTypeNameString).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetTypeNameString(RecursiveGenericTypeGetTypeNameString sut)
        {
            Assert.IsAssignableFrom<IGetTypeNameString>(sut);
        }

        [Theory, AutoNSubstituteData]
        public void For_TypeHasNullGuard(RecursiveGenericTypeGetTypeNameString sut)
        {
            // Arrange
            string @out;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.For(null, out @out));
        }

        [Theory, AutoNSubstituteData]
        public void For_NonGenericType_ReturnsFalse(RecursiveGenericTypeGetTypeNameString sut)
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
        public void For_GenericTypeOneParameter_ReturnsTrueAndTypeNameWithGenericArgumentFromInnerInstance(
            [Frozen] IGetTypeNameString innerGetTypeNameString,
            RecursiveGenericTypeGetTypeNameString sut,
            string genericArgumentName)
        {
            // Arrange
            var @out = Arg.Any<string>();
            innerGetTypeNameString.For(typeof(AutoNSubstituteDataAttribute), out @out).Returns(x =>
                                                                                               {
                                                                                                   x[1] = genericArgumentName;
                                                                                                   return true;
                                                                                               });

            var type = typeof(IList<AutoNSubstituteDataAttribute>);

            var expectedTypeName = "IList<" + genericArgumentName + ">";

            // Act
            string resultTypeName;
            var result = sut.For(type, out resultTypeName);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedTypeName, resultTypeName);
        }

        [Theory, AutoNSubstituteData]
        public void For_GenericTypeThreeParameters_ReturnsTrueAndTypeNameWithGenericArgumentsFromInnerInstance(
            [Frozen] IGetTypeNameString innerGetTypeNameString,
            RecursiveGenericTypeGetTypeNameString sut,
            IFixture fixture)
        {
            // Arrange
            var genericArgumentName1 = fixture.Create<string>();
            var genericArgumentName2 = fixture.Create<string>();
            var genericArgumentName3 = fixture.Create<string>();

            var @out = Arg.Any<string>();
            innerGetTypeNameString.For(typeof(AutoNSubstituteDataAttribute), out @out).Returns(x =>
                                                                                               {
                                                                                                   x[1] = genericArgumentName1;
                                                                                                   return true;
                                                                                               });

            @out = Arg.Any<string>();
            innerGetTypeNameString.For(typeof(Int64), out @out).Returns(x =>
                                                                        {
                                                                            x[1] = genericArgumentName2;
                                                                            return true;
                                                                        });

            @out = Arg.Any<string>();
            innerGetTypeNameString.For(typeof(InsufficientMemoryException), out @out).Returns(x =>
                                                                                              {
                                                                                                  x[1] = genericArgumentName3;
                                                                                                  return true;
                                                                                              });

            var type = typeof(Tuple<AutoNSubstituteDataAttribute, Int64, InsufficientMemoryException>);

            var expectedTypeName = "Tuple<" + genericArgumentName1 + ", " + genericArgumentName2 + ", " + genericArgumentName3 + ">";

            // Act
            string resultTypeName;
            var result = sut.For(type, out resultTypeName);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedTypeName, resultTypeName);
        }
    }
}