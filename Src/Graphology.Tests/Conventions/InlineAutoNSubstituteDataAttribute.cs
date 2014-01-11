using Ploeh.AutoFixture.Xunit;

namespace TeaDriven.Graphology.Tests.Conventions
{
    public class InlineAutoNSubstituteDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoNSubstituteDataAttribute(params object[] values)
            : base(new AutoNSubstituteDataAttribute(), values)
        {
        }
    }
}