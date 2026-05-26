using System;
using Xunit;
using OnboardingSystem.Services;

namespace OnboardingSystem.Tests.Unit
{
    /// <summary>
    /// Unit тесты для PasswordHasher
    /// 
    /// Проверяют:
    /// - Хеширование паролей
    /// - Верификацию паролей
    /// - Валидацию требований к паролям
    /// </summary>
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new();

        [Fact]
        public void HashPassword_WithValidPassword_ReturnsHash()
        {
            // Arrange
            var password = "ValidPassword123!@#";

            // Act
            var hash = _hasher.HashPassword(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.NotEqual(password, hash);
        }

        [Fact]
        public void HashPassword_WithSamePassword_ReturnsDifferentHashes()
        {
            // Arrange
            var password = "SamePassword123!@#";

            // Act
            var hash1 = _hasher.HashPassword(password);
            var hash2 = _hasher.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2); // BCrypt добавляет random salt
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "CorrectPassword123!@#";
            var hash = _hasher.HashPassword(password);

            // Act
            var result = _hasher.VerifyPassword(password, hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
        {
            // Arrange
            var password = "CorrectPassword123!@#";
            var wrongPassword = "WrongPassword123!@#";
            var hash = _hasher.HashPassword(password);

            // Act
            var result = _hasher.VerifyPassword(wrongPassword, hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_WithEmptyPassword_ReturnsFalse()
        {
            // Arrange
            var password = "ValidPassword123!@#";
            var hash = _hasher.HashPassword(password);

            // Act
            var result = _hasher.VerifyPassword("", hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_WithEmptyHash_ReturnsFalse()
        {
            // Arrange
            var password = "ValidPassword123!@#";

            // Act
            var result = _hasher.VerifyPassword(password, "");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void HashPassword_WithEmptyPassword_ThrowsException(string? password)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _hasher.HashPassword(password ?? ""));
        }

        [Fact]
        public void HashPassword_WithShortPassword_ThrowsException()
        {
            // Arrange
            var password = "Short1!"; // 7 символов, меньше 8

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _hasher.HashPassword(password));
        }

        [Fact]
        public void VerifyPassword_WithNullPassword_ReturnsFalse()
        {
            // Arrange
            var hash = _hasher.HashPassword("ValidPassword123!@#");

            // Act
            var result = _hasher.VerifyPassword(null ?? "", hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_WithNullHash_ReturnsFalse()
        {
            // Arrange
            var password = "ValidPassword123!@#";

            // Act
            var result = _hasher.VerifyPassword(password, null ?? "");

            // Assert
            Assert.False(result);
        }
    }
}
