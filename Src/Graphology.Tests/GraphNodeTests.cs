using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Idioms;
using System.Linq;
using TeaDriven.Graphology.Tests.Conventions;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests
{
    public static class FixtureExtensions
    {
        public static void MakeNonRecursive(this IFixture fixture)
        {
            fixture.Behaviors.Remove(fixture.Behaviors.First(b => b is ThrowingRecursionBehavior));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
    }

    public interface IAbstraction1
    {
    }

    public interface IAbstraction2
    {
    }

    public class NoDependencyType : IAbstraction1, IAbstraction2
    {
    }

    public class GraphNodeTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(GraphNode).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Properties_AreWritable(WritablePropertyAssertion assertion, IFixture fixture)
        {
            fixture.MakeNonRecursive();

            assertion.Verify(typeof(GraphNode));
        }

        [Theory, AutoNSubstituteData]
        public void SubGraph_NotSet_ReturnsEmptyList(IFixture fixture)
        {
            // Arrange
            fixture.MakeNonRecursive();

            fixture.Customize(new NoAutoPropertiesCustomization(typeof(GraphNode)));

            var sut = fixture.Create<GraphNode>();

            // Act
            // Assert
            Assert.Empty(sut.SubGraph);
        }
    }
}