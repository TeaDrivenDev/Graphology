using NSubstitute;
using System.Linq;
using TeaDriven.Graphology.Traversal;
using Xunit;

namespace TeaDriven.Graphology.Tests.Traversal
{
    public class TypeExclusionsTests
    {
        private class SutExcludeTypeExclusions : TypeExclusions
        {
            public SutExcludeTypeExclusions(ITypeExclusion excludeFixed)
            {
                this._excludeFixed = excludeFixed;
            }
        }

        [Fact]
        public void Exclude_NoAdditionalExclusions_ReturnsExcludeFixed()
        {
            // Arrange
            var expectedResult = Substitute.For<ITypeExclusion>();

            var sut = new SutExcludeTypeExclusions(expectedResult);

            // Act
            var result = sut.Exclude;

            // Assert
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void Exclude_AdditionalExclusions_ReturnsCompositeTypeExclusion()
        {
            // Arrange
            var additional = Substitute.For<ITypeExclusion>();

            var excludeFixed = Substitute.For<ITypeExclusion>();
            var sut = new SutExcludeTypeExclusions(excludeFixed)
                      {
                          Exclude = additional
                      };

            // Act
            var result = (CompositeTypeExclusion)sut.Exclude;

            // Assert
            Assert.Equal(2, result.TypeExclusions.Count());
            Assert.Contains(excludeFixed, result.TypeExclusions);
            Assert.Contains(additional, result.TypeExclusions);
        }

        private class SutDoNotFollowTypeExclusions : TypeExclusions
        {
            public SutDoNotFollowTypeExclusions(ITypeExclusion doNotFollowFixed)
            {
                this._doNotFollowFixed = doNotFollowFixed;
            }
        }

        [Fact]
        public void DoNotFollow_NoAdditionalExclusions_ReturnsDoNotFollowFixed()
        {
            // Arrange
            var expectedResult = Substitute.For<ITypeExclusion>();

            var sut = new SutDoNotFollowTypeExclusions(expectedResult);

            // Act
            var result = sut.DoNotFollow;

            // Assert
            Assert.Same(expectedResult, result);
        }

        [Fact]
        public void DoNotFollow_AdditionalExclusions_ReturnsCompositeTypeExclusion()
        {
            // Arrange
            var additional = Substitute.For<ITypeExclusion>();

            var doNotFollowFixed = Substitute.For<ITypeExclusion>();
            var sut = new SutDoNotFollowTypeExclusions(doNotFollowFixed)
                      {
                          DoNotFollow = additional
                      };

            // Act
            var result = (CompositeTypeExclusion)sut.DoNotFollow;

            // Assert
            Assert.Equal(2, result.TypeExclusions.Count());
            Assert.Contains(doNotFollowFixed, result.TypeExclusions);
            Assert.Contains(additional, result.TypeExclusions);
        }
    }
}