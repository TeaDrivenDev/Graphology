using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;
using TeaDriven.Graphology.Tests.Conventions;
using TeaDriven.Graphology.Visualization;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests.Visualization
{
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