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
    public class DefaultGetNodeStringTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(DefaultGetNodeString).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetNodeString(DefaultGetNodeString sut)
        {
            Assert.IsAssignableFrom<IGetNodeString>(sut);
        }

        [Theory, AutoNSubstituteData]
        public void For_HasNullGuards(GuardClauseAssertion assertion, IFixture fixture)
        {
            // Arrange
            fixture.MakeNonRecursive();

            // Act
            // Assert
            assertion.Verify(typeof(DefaultGetNodeString).GetMethod("For"));
        }

        [Theory, AutoNSubstituteData]
        public void For_ReturnsCombinationOfDepthAndMemberTypesStrings(
            [Frozen] IGetDepthString getDepthString,
            [Frozen] IGetMemberTypesString getMemberTypesString,
            string depthString,
            string memberTypesString,
            int depth,
            IFixture fixture)
        {
            // Arrange
            fixture.MakeNonRecursive();
            fixture.Customize(new NoAutoPropertiesCustomization(typeof(DefaultGetNodeString)));
            fixture.Customize(new NoAutoPropertiesCustomization(typeof(GraphNode)));

            var sut = fixture.Create<DefaultGetNodeString>();

            var graphNode = fixture.Create<GraphNode>();

            getDepthString.For(depth).Returns(depthString);
            getMemberTypesString.For(graphNode).Returns(memberTypesString);

            var expectedResult = depthString + memberTypesString;

            // Act
            var result = sut.For(graphNode, depth);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}