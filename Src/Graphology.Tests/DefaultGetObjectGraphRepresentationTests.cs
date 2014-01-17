using Ploeh.AutoFixture.Idioms;
using TeaDriven.Graphology.Tests.Conventions;
using Xunit;
using Xunit.Extensions;

namespace TeaDriven.Graphology.Tests
{
    public class DefaultGetObjectGraphRepresentationTests
    {
        [Theory, AutoNSubstituteData]
        public void Constructor_HasNullGuards(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(DefaultGetObjectGraph).GetConstructors());
        }

        [Theory, AutoNSubstituteData]
        public void Constructor_ReturnsIGetObjectGraphRepresentation(DefaultGetObjectGraph sut)
        {
            Assert.IsAssignableFrom<IGetObjectGraph>(sut);
        }

        [Theory, AutoNSubstituteData]
        public void For_ObjectWithoutDependencies_ReturnsCorrectGraphNode(DefaultGetObjectGraph sut,
                                                                          NoDependencyType currentObject)
        {
        }
    }
}