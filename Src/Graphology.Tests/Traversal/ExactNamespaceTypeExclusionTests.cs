using TeaDriven.Graphology.Traversal;
using Xunit;

namespace TeaDriven.Graphology.Tests.Traversal
{
    public class ExactNamespaceTypeExclusionTests
    {
        [Fact]
        public void AppliesTo_TypeInExactNamespace_ReturnsTrue()
        {
            // Arrange
            var sut = new ExactNamespaceTypeExclusion("System");

            // Act
            var result = sut.AppliesTo(typeof(System.Int32));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AppliesTo_TypeInSubordinateNamespace_ReturnsFalse()
        {
            // Arrange
            var sut = new ExactNamespaceTypeExclusion("System");

            // Act
            var result = sut.AppliesTo(typeof(System.Collections.Generic.List<string>));

            // Assert
            Assert.False(result);
        }
    }
}