using FluentAssertions;
using CompanyManager.Application.Services;
using System;
using System.Collections.Generic;

namespace CompanyManager.UnitTest.Services
{
    public class PasswordHasherTest
    {
        private readonly PasswordHasher _passwordHasher;

        public PasswordHasherTest()
        {
            _passwordHasher = new PasswordHasher();
        }

        //[Fact(DisplayName = "Should hash password and return non-empty hash different from input")]
        //public void Should_Hash_Password_And_Return_Non_Empty_Hash_Different_From_Input()
        //{
        //    // Arrange
        //    var plainTextPassword = "MySecurePassword123!";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(plainTextPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(plainTextPassword);
        //    hashedPassword.Length.Should().BeGreaterThan(plainTextPassword.Length);
        //}

        //[Theory(DisplayName = "Should hash different passwords and return different hashes")]
        //[InlineData("password123", "password456")]
        //[InlineData("admin123", "user123")]
        //[InlineData("simple", "complexPassword!@#")]
        //[InlineData("12345678", "87654321")]
        //public void Should_Hash_Different_Passwords_And_Return_Different_Hashes(string password1, string password2)
        //{
        //    // Arrange
        //    // Act
        //    var hash1 = _passwordHasher.HashPassword(password1);
        //    var hash2 = _passwordHasher.HashPassword(password2);

        //    // Assert
        //    hash1.Should().NotBe(hash2);
        //    hash1.Should().NotBe(password1);
        //    hash2.Should().NotBe(password2);
        //}

        //[Fact(DisplayName = "Should hash same password multiple times and return different hashes")]
        //public void Should_Hash_Same_Password_Multiple_Times_And_Return_Different_Hashes()
        //{
        //    // Arrange
        //    var plainTextPassword = "MySecurePassword123!";

        //    // Act
        //    var hash1 = _passwordHasher.HashPassword(plainTextPassword);
        //    var hash2 = _passwordHasher.HashPassword(plainTextPassword);
        //    var hash3 = _passwordHasher.HashPassword(plainTextPassword);

        //    // Assert
        //    hash1.Should().NotBe(hash2);
        //    hash1.Should().NotBe(hash3);
        //    hash2.Should().NotBe(hash3);
            
        //    // All hashes should be different from original password
        //    hash1.Should().NotBe(plainTextPassword);
        //    hash2.Should().NotBe(plainTextPassword);
        //    hash3.Should().NotBe(plainTextPassword);
        //}

        //[Fact(DisplayName = "Should verify password correctly when password matches")]
        //public void Should_Verify_Password_Correctly_When_Password_Matches()
        //{
        //    // Arrange
        //    var plainTextPassword = "MySecurePassword123!";
        //    var hashedPassword = _passwordHasher.HashPassword(plainTextPassword);

        //    // Act
        //    var result = _passwordHasher.VerifyPassword(plainTextPassword, hashedPassword);

        //    // Assert
        //    result.Should().BeTrue();
        //}

        //[Theory(DisplayName = "Should verify various correct passwords successfully")]
        //[InlineData("password123")]
        //[InlineData("Admin@123")]
        //[InlineData("VeryLongPasswordWithSpecialChars!@#$%^&*()")]
        //[InlineData("123456789")]
        //[InlineData("a")]
        //public void Should_Verify_Various_Correct_Passwords_Successfully(string plainTextPassword)
        //{
        //    // Arrange
        //    var hashedPassword = _passwordHasher.HashPassword(plainTextPassword);

        //    // Act
        //    var result = _passwordHasher.VerifyPassword(plainTextPassword, hashedPassword);

        //    // Assert
        //    result.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should verify password correctly when password does not match")]
        //public void Should_Verify_Password_Correctly_When_Password_Does_Not_Match()
        //{
        //    // Arrange
        //    var correctPassword = "MySecurePassword123!";
        //    var incorrectPassword = "WrongPassword456!";
        //    var hashedPassword = _passwordHasher.HashPassword(correctPassword);

        //    // Act
        //    var result = _passwordHasher.VerifyPassword(incorrectPassword, hashedPassword);

        //    // Assert
        //    result.Should().BeFalse();
        //}

        //[Theory(DisplayName = "Should reject various incorrect passwords")]
        //[InlineData("password123", "password456")]
        //[InlineData("admin123", "admin456")]
        //[InlineData("user@123", "user@456")]
        //[InlineData("12345678", "87654321")]
        //[InlineData("simple", "complex")]
        //public void Should_Reject_Various_Incorrect_Passwords(string correctPassword, string incorrectPassword)
        //{
        //    // Arrange
        //    var hashedPassword = _passwordHasher.HashPassword(correctPassword);

        //    // Act
        //    var result = _passwordHasher.VerifyPassword(incorrectPassword, hashedPassword);

        //    // Assert
        //    result.Should().BeFalse();
        //}

        //[Fact(DisplayName = "Should handle empty password correctly")]
        //public void Should_Handle_Empty_Password_Correctly()
        //{
        //    // Arrange
        //    var emptyPassword = "";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(emptyPassword);
        //    var verificationResult = _passwordHasher.VerifyPassword(emptyPassword, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(emptyPassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should handle whitespace password correctly")]
        //public void Should_Handle_Whitespace_Password_Correctly()
        //{
        //    // Arrange
        //    var whitespacePassword = "   ";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(whitespacePassword);
        //    var verificationResult = _passwordHasher.VerifyPassword(whitespacePassword, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(whitespacePassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should handle null password correctly")]
        //public void Should_Handle_Null_Password_Correctly()
        //{
        //    // Arrange
        //    string? nullPassword = null;

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(nullPassword!);
        //    var verificationResult = _passwordHasher.VerifyPassword(nullPassword!, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(nullPassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should handle very long password correctly")]
        //public void Should_Handle_Very_Long_Password_Correctly()
        //{
        //    // Arrange
        //    var longPassword = new string('a', 1000) + "!@#$%^&*()";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(longPassword);
        //    var verificationResult = _passwordHasher.VerifyPassword(longPassword, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(longPassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should handle special characters in password correctly")]
        //public void Should_Handle_Special_Characters_In_Password_Correctly()
        //{
        //    // Arrange
        //    var specialCharPassword = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(specialCharPassword);
        //    var verificationResult = _passwordHasher.VerifyPassword(specialCharPassword, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(specialCharPassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should handle unicode characters in password correctly")]
        //public void Should_Handle_Unicode_Characters_In_Password_Correctly()
        //{
        //    // Arrange
        //    var unicodePassword = "SenhaComAcentosáéíóúçãõ";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(unicodePassword);
        //    var verificationResult = _passwordHasher.VerifyPassword(unicodePassword, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(unicodePassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should handle numeric password correctly")]
        //public void Should_Handle_Numeric_Password_Correctly()
        //{
        //    // Arrange
        //    var numericPassword = "12345678901234567890";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(numericPassword);
        //    var verificationResult = _passwordHasher.VerifyPassword(numericPassword, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(numericPassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should handle single character password correctly")]
        //public void Should_Handle_Single_Character_Password_Correctly()
        //{
        //    // Arrange
        //    var singleCharPassword = "a";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(singleCharPassword);
        //    var verificationResult = _passwordHasher.VerifyPassword(singleCharPassword, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(singleCharPassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should handle mixed case password correctly")]
        //public void Should_Handle_Mixed_Case_Password_Correctly()
        //{
        //    // Arrange
        //    var mixedCasePassword = "MyPassword123!@#";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(mixedCasePassword);
        //    var verificationResult = _passwordHasher.VerifyPassword(mixedCasePassword, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(mixedCasePassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should handle edge case with very short password")]
        //public void Should_Handle_Edge_Case_With_Very_Short_Password()
        //{
        //    // Arrange
        //    var shortPassword = "ab";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(shortPassword);
        //    var verificationResult = _passwordHasher.VerifyPassword(shortPassword, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(shortPassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should maintain consistency between hash and verify operations")]
        //public void Should_Maintain_Consistency_Between_Hash_And_Verify_Operations()
        //{
        //    // Arrange
        //    var password = "ConsistentPassword123!";
        //    var hashedPassword = _passwordHasher.HashPassword(password);

        //    // Act & Assert
        //    for (int i = 0; i < 10; i++)
        //    {
        //        var result = _passwordHasher.VerifyPassword(password, hashedPassword);
        //        result.Should().BeTrue($"Password verification should succeed on attempt {i + 1}");
        //    }
        //}

        //[Fact(DisplayName = "Should handle password with only special characters")]
        //public void Should_Handle_Password_With_Only_Special_Characters()
        //{
        //    // Arrange
        //    var specialOnlyPassword = "!@#$%^&*()";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(specialOnlyPassword);
        //    var verificationResult = _passwordHasher.VerifyPassword(specialOnlyPassword, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(specialOnlyPassword);
        //    verificationResult.Should().BeTrue();
        //}

        //[Fact(DisplayName = "Should handle password with spaces in the middle")]
        //public void Should_Handle_Password_With_Spaces_In_The_Middle()
        //{
        //    // Arrange
        //    var passwordWithSpaces = "My Password 123";

        //    // Act
        //    var hashedPassword = _passwordHasher.HashPassword(passwordWithSpaces);
        //    var verificationResult = _passwordHasher.VerifyPassword(passwordWithSpaces, hashedPassword);

        //    // Assert
        //    hashedPassword.Should().NotBeNullOrEmpty();
        //    hashedPassword.Should().NotBe(passwordWithSpaces);
        //    verificationResult.Should().BeTrue();
        //}
    }
}
