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

    public class DefaultGetMemberTypesStringTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(DefaultGetMemberTypesString).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetMemberTypesString(DefaultGetMemberTypesString sut)
        {
            Assert.IsAssignableFrom<IGetMemberTypesString>(sut);
        }

        [Theory, AutoNSubstituteData]
        public void For_HasNullGuards(GuardClauseAssertion assertion, IFixture fixture)
        {
            // Arrange
            fixture.MakeNonRecursive();

            // Act
            // Assert
            assertion.Verify(typeof(DefaultGetMemberTypesString).GetMethod("For"));
        }

        [Theory, AutoNSubstituteData]
        public void For_DefaultFormatString_ReturnsCombinationOfObjectTypeNameAndReferenceTypeName(
            [Frozen] IGetTypeNameString getTypeNameString, string objectTypeName, string referenceTypeName, IFixture fixture)
        {
            // Arrange
            fixture.Customize(new NoAutoPropertiesCustomization(typeof(DefaultGetMemberTypesString)));

            var objectType = typeof(AutoNSubstituteDataAttribute);
            var referenceType = typeof(CustomizationNode);

            var graphNode = new GraphNode()
                            {
                                ReferenceType = referenceType,
                                ObjectType = objectType
                            };

            var @out = Arg.Any<string>();
            getTypeNameString.For(objectType, out @out).Returns((x =>
                                                                 {
                                                                     x[1] = objectTypeName;
                                                                     return true;
                                                                 }));
            @out = Arg.Any<string>();
            getTypeNameString.For(referenceType, out @out).Returns((x =>
                                                                 {
                                                                     x[1] = referenceTypeName;
                                                                     return true;
                                                                 }));

            var expectedResult = objectTypeName + " : " + referenceTypeName;

            var sut = fixture.Create<DefaultGetMemberTypesString>();

            // Act
            var result = sut.For(graphNode);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}