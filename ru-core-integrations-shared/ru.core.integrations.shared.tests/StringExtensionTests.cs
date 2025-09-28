using ru.core.integrations.shared.Services.Extensions;

namespace ru.core.integrations.shared.tests
{
    public class StringExtensionTests
    {
        [Fact]
        public void EqualsIgnoreCase_ShouldReturnTrue_ForSameStringsWithDifferentCases()
        {
            // Arrange
            string str1 = "HelloWorld";
            string str2 = "helloworld";
            // Act
            bool result = str1.EqualsIgnoreCase(str2);
            // Assert
            Assert.True(result);
        }
        [Fact]
        public void EqualsIgnoreCase_ShouldReturnFalse_ForDifferentStrings()
        {
            // Arrange
            string str1 = "HelloWorld";
            string str2 = "HelloUniverse";
            // Act
            bool result = str1.EqualsIgnoreCase(str2);
            // Assert
            Assert.False(result);
        }
        [Fact]
        public void IsNullOrWhitespace_ShouldReturnTrue_ForNullOrWhitespaceStrings()
        {
            // Arrange
            string? str1 = null;
            string str2 = "   ";
            // Act & Assert
            Assert.True(str1.IsNullOrWhitespace());
            Assert.True(str2.IsNullOrWhitespace());
        }
        [Fact]
        public void IsNullOrWhitespace_ShouldReturnFalse_ForNonWhitespaceStrings()
        {
            // Arrange
            string str = "Hello";
            // Act
            bool result = str.IsNullOrWhitespace();
            // Assert
            Assert.False(result);
        }
        [Fact]
        public void LimitToMaxLength_ShouldTruncateString_WhenExceedingMaxLength()
        {
            // Arrange
            string str = "HelloWorld";
            int maxLength = 5;
            // Act
            string? result = str.LimitToMaxLength(maxLength);
            // Assert
            Assert.Equal("Hello", result);
        }

        [Fact]
        public void LimitToMaxLength_ShouldReturnOriginalString_WhenWithinMaxLength()
        {
            // Arrange
            string str = "Hello";
            int maxLength = 10;
            // Act
            string? result = str.LimitToMaxLength(maxLength);
            // Assert
            Assert.Equal(str, result);
        }

        [Fact]
        public void LimitToMaxLength_ShouldReturnNull_WhenOriginalStringIsNull()
        {
            // Arrange
            string str = null;
            int maxLength = 10;
            // Act
            string? result = str.LimitToMaxLength(maxLength);
            // Assert
            Assert.Equal(str, null);
        }
    }
}
