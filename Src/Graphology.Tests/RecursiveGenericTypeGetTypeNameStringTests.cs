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

    public class CompositeGetTypeNameStringTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(CompositeGetTypeNameString).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetTypeNameString(CompositeGetTypeNameString sut)
        {
            Assert.IsAssignableFrom<IGetTypeNameString>(sut);
        }

        [Theory, AutoNSubstituteData]
        public void For_TypeHasNullGuard(CompositeGetTypeNameString sut)
        {
            // Arrange
            string @out;

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.For(null, out @out));
        }

        [Theory, AutoNSubstituteData]
        public void For_EmptyInnerList_ReturnsFalse(IFixture fixture, Type type)
        {
            // Arrange
            fixture.Inject<IEnumerable<IGetTypeNameString>>(new List<IGetTypeNameString>());

            var sut = fixture.Create<CompositeGetTypeNameString>();

            var expectedResult = false;

            // Act
            string resultTypeName;
            var result = sut.For(type, out resultTypeName);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory, AutoNSubstituteData]
        public void For_NoInnerInstanceHandles_ReturnsFalse([Frozen] IEnumerable<IGetTypeNameString> innerInstances,
                                                            [Frozen] CompositeGetTypeNameString sut,
                                                            Type type,
                                                            IFixture fixture)
        {
            // Arrange
            foreach (var innerInstance in innerInstances)
            {
                var typeName = "";
                innerInstance.For(type, out typeName).Returns(x =>
                                                              {
                                                                  x[1] = fixture.Create<string>();
                                                                  return false;
                                                              });
            }

            var expectedResult = false;

            // Act
            string resultTypeName;
            var result = sut.For(type, out resultTypeName);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory, AutoNSubstituteData]
        public void For_FirstInnerInstanceHandles_ReturnsTrueAndTypeName(
            List<IGetTypeNameString> innerInstances,
            IGetTypeNameString firstInnerInstance,
            string expectedTypeName,
            Type type,
            IFixture fixture)
        {
            // Arrange
            var @out = Arg.Any<string>();

            firstInnerInstance.For(type, out @out).Returns(x =>
                                                            {
                                                                x[1] = expectedTypeName;
                                                                return true;
                                                            });

            foreach (var innerInstance in innerInstances)
            {
                @out = Arg.Any<string>();
                innerInstance.For(type, out  @out).ReturnsForAnyArgs(x => { throw new Exception(); });
            }

            innerInstances.Insert(0, firstInnerInstance);

            fixture.Inject<IGetTypeNameString[]>(innerInstances.ToArray());

            var sut = fixture.Create<CompositeGetTypeNameString>();

            var expectedResult = true;

            // Act
            string resultTypeName;
            var result = sut.For(type, out resultTypeName);

            // Assert
            Assert.Equal(expectedTypeName, resultTypeName);
            Assert.Equal(expectedResult, result);
        }

        [Theory, AutoNSubstituteData]
        public void For_LastInnerInstanceHandles_ReturnsTrueAndTypeName(
            List<IGetTypeNameString> innerInstances,
            IGetTypeNameString lastInnerInstance,
            string expectedTypeName,
            Type type,
            IFixture fixture)
        {
            // Arrange
            var @out = Arg.Any<string>();

            lastInnerInstance.For(type, out @out).Returns(x =>
                                                            {
                                                                x[1] = expectedTypeName;
                                                                return true;
                                                            });

            foreach (var innerInstance in innerInstances)
            {
                @out = Arg.Any<string>();
                innerInstance.For(type, out @out).ReturnsForAnyArgs(false);
            }

            innerInstances.Add(lastInnerInstance);

            fixture.Inject<IGetTypeNameString[]>(innerInstances.ToArray());

            var sut = fixture.Create<CompositeGetTypeNameString>();

            var expectedResult = true;

            // Act
            string resultTypeName;
            var result = sut.For(type, out resultTypeName);

            // Assert
            Assert.Equal(expectedTypeName, resultTypeName);
            Assert.Equal(expectedResult, result);
        }
    }
}