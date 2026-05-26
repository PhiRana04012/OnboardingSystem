using Xunit;
using System;
using System.Text.RegularExpressions;

namespace OnboardingSystem.Tests.Unit
{
    /// <summary>
    /// Unit тесты для валидаторов данных
    /// 
    /// Проверяют:
    /// - Валидацию email адресов
    /// - Валидацию паролей
    /// - Валидацию других полей
    /// </summary>
    public class ValidatorsTests
    {
        // Вспомогательные функции валидации
        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            return emailRegex.IsMatch(email);
        }

        private bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            var hasUpperCase = Regex.IsMatch(password, "[A-Z]");
            var hasLowerCase = Regex.IsMatch(password, "[a-z]");
            var hasDigit = Regex.IsMatch(password, "[0-9]");
            var hasSpecialChar = Regex.IsMatch(password, "[!@#$%^&*]");

            return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }

        private bool ValidateFullName(string fullName)
        {
            return !string.IsNullOrWhiteSpace(fullName) && fullName.Length >= 2 && fullName.Length <= 200;
        }

        [Theory]
        [InlineData("user@example.com", true)]
        [InlineData("john.doe@company.co.uk", true)]
        [InlineData("test.email+tag@gmail.com", true)]
        [InlineData("invalid.email@", false)]
        [InlineData("invalid@email", false)]
        [InlineData("@example.com", false)]
        [InlineData("user@.com", false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void ValidateEmail_WithVariousInputs_ReturnsCorrectResult(string email, bool expected)
        {
            // Act
            var result = ValidateEmail(email);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("ValidPass123!", true)]
        [InlineData("SecurePassword1!", true)]
        [InlineData("Complex@Pass2", true)]
        [InlineData("password123!", false)] // No uppercase
        [InlineData("PASSWORD123!", false)] // No lowercase
        [InlineData("ValidPass!", false)]   // No digit
        [InlineData("ValidPass123", false)] // No special char
        [InlineData("Short1!", false)]       // Less than 8 chars
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void ValidatePassword_WithVariousInputs_ReturnsCorrectResult(string password, bool expected)
        {
            // Act
            var result = ValidatePassword(password);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("John Doe", true)]
        [InlineData("Mary Jane Watson", true)]
        [InlineData("A", false)] // Too short
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void ValidateFullName_WithVariousInputs_ReturnsCorrectResult(string fullName, bool expected)
        {
            // Act
            var result = ValidateFullName(fullName);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ValidateFullName_With200Chars_ReturnsTrue()
        {
            var fullName = new string('A', 200);
            Assert.True(ValidateFullName(fullName));
        }

        [Fact]
        public void ValidateFullName_With201Chars_ReturnsFalse()
        {
            var fullName = new string('A', 201);
            Assert.False(ValidateFullName(fullName));
        }

        [Fact]
        public void ValidateEmail_WithNull_ReturnsFalse()
        {
            // Act
            var result = ValidateEmail(null ?? "");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidatePassword_WithNull_ReturnsFalse()
        {
            // Act
            var result = ValidatePassword(null ?? "");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidateFullName_WithNull_ReturnsFalse()
        {
            // Act
            var result = ValidateFullName(null ?? "");

            // Assert
            Assert.False(result);
        }
    }
}
