using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit;
using TeaDriven.Graphology.Tests.Conventions;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests
{
    public class NewGetNodeStringTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof (NewGetNodeString).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetNodeString(NewGetNodeString sut)
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
            assertion.Verify(typeof(NewGetNodeString).GetMethod("For"));
        }

        [Theory, AutoNSubstituteData]
        public void For_ReturnsCombinationOfDepthAndMemberTypesStrings(
            [Frozen] IGetDepthString getDepthString,
            [Frozen] IGetMemberTypesString getMemberTypesString,
            NewGetNodeString sut,
            string depthString,
            string memberTypesString,
            int depth,
            IFixture fixture)
        {
            // Arrange
            fixture.MakeNonRecursive();
            
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