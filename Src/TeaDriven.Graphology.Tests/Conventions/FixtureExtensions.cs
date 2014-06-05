using System.Linq;
using Ploeh.AutoFixture;

namespace TeaDriven.Graphology.Tests.Conventions
{
    public static class FixtureExtensions
    {
        public static void MakeNonRecursive(this IFixture fixture)
        {
            fixture.Behaviors.Remove(fixture.Behaviors.First(b => b is ThrowingRecursionBehavior));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
    }
}