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

        [Fact(DisplayName = "Should hash password and return non-empty hash different from input")]
        public void Should_Hash_Password_And_Return_Non_Empty_Hash_Different_From_Input()
        {
            var plainTextPassword = "MySecurePassword123!";

            var hashedPassword = _passwordHasher.Hash(plainTextPassword);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(plainTextPassword);
            hashedPassword.Length.Should().BeGreaterThan(plainTextPassword.Length);
        }

        [Theory(DisplayName = "Should hash different passwords and return different hashes")]
        [InlineData("password123", "password456")]
        [InlineData("admin123", "user123")]
        [InlineData("simple", "complexPassword!@#")]
        [InlineData("12345678", "87654321")]
        public void Should_Hash_Different_Passwords_And_Return_Different_Hashes(string password1, string password2)
        {
            var hash1 = _passwordHasher.Hash(password1);
            var hash2 = _passwordHasher.Hash(password2);

            hash1.Should().NotBe(hash2);
            hash1.Should().NotBe(password1);
            hash2.Should().NotBe(password2);
        }

        [Fact(DisplayName = "Should hash same password multiple times and return different hashes")]
        public void Should_Hash_Same_Password_Multiple_Times_And_Return_Different_Hashes()
        {
            var plainTextPassword = "MySecurePassword123!";

            var hash1 = _passwordHasher.Hash(plainTextPassword);
            var hash2 = _passwordHasher.Hash(plainTextPassword);
            var hash3 = _passwordHasher.Hash(plainTextPassword);

            hash1.Should().NotBe(hash2);
            hash1.Should().NotBe(hash3);
            hash2.Should().NotBe(hash3);
            
            hash1.Should().NotBe(plainTextPassword);
            hash2.Should().NotBe(plainTextPassword);
            hash3.Should().NotBe(plainTextPassword);
        }

        [Fact(DisplayName = "Should verify password correctly when password matches")]
        public void Should_Verify_Password_Correctly_When_Password_Matches()
        {
            var plainTextPassword = "MySecurePassword123!";
            var hashedPassword = _passwordHasher.Hash(plainTextPassword);

            var result = _passwordHasher.Verify(plainTextPassword, hashedPassword);

            result.Should().BeTrue();
        }

        [Theory(DisplayName = "Should verify various correct passwords successfully")]
        [InlineData("password123")]
        [InlineData("Admin@123")]
        [InlineData("VeryLongPasswordWithSpecialChars!@#$%^&*()")]
        [InlineData("123456789")]
        [InlineData("a")]
        public void Should_Verify_Various_Correct_Passwords_Successfully(string plainTextPassword)
        {
            var hashedPassword = _passwordHasher.Hash(plainTextPassword);

            var result = _passwordHasher.Verify(plainTextPassword, hashedPassword);

            result.Should().BeTrue();
        }

        [Fact(DisplayName = "Should verify password correctly when password does not match")]
        public void Should_Verify_Password_Correctly_When_Password_Does_Not_Match()
        {
            var correctPassword = "MySecurePassword123!";
            var incorrectPassword = "WrongPassword456!";
            var hashedPassword = _passwordHasher.Hash(correctPassword);

            var result = _passwordHasher.Verify(incorrectPassword, hashedPassword);

            result.Should().BeFalse();
        }

        [Theory(DisplayName = "Should reject various incorrect passwords")]
        [InlineData("password123", "password456")]
        [InlineData("admin123", "admin456")]
        [InlineData("user@123", "user@456")]
        [InlineData("12345678", "87654321")]
        [InlineData("simple", "complex")]
        public void Should_Reject_Various_Incorrect_Passwords(string correctPassword, string incorrectPassword)
        {
            var hashedPassword = _passwordHasher.Hash(correctPassword);

            var result = _passwordHasher.Verify(incorrectPassword, hashedPassword);

            result.Should().BeFalse();
        }

        [Fact(DisplayName = "Should handle very long password correctly")]
        public void Should_Handle_Very_Long_Password_Correctly()
        {
            var longPassword = new string('a', 1000) + "!@#$%^&*()";

            var hashedPassword = _passwordHasher.Hash(longPassword);
            var verificationResult = _passwordHasher.Verify(longPassword, hashedPassword);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(longPassword);
            verificationResult.Should().BeTrue();
        }

        [Fact(DisplayName = "Should handle special characters in password correctly")]
        public void Should_Handle_Special_Characters_In_Password_Correctly()
        {
            var specialCharPassword = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var hashedPassword = _passwordHasher.Hash(specialCharPassword);
            var verificationResult = _passwordHasher.Verify(specialCharPassword, hashedPassword);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(specialCharPassword);
            verificationResult.Should().BeTrue();
        }

        [Fact(DisplayName = "Should handle unicode characters in password correctly")]
        public void Should_Handle_Unicode_Characters_In_Password_Correctly()
        {
            var unicodePassword = "SenhaComAcentosáéíóúçãõ";

            var hashedPassword = _passwordHasher.Hash(unicodePassword);
            var verificationResult = _passwordHasher.Verify(unicodePassword, hashedPassword);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(unicodePassword);
            verificationResult.Should().BeTrue();
        }

        [Fact(DisplayName = "Should handle numeric password correctly")]
        public void Should_Handle_Numeric_Password_Correctly()
        {
            var numericPassword = "52998224725234567890";

            var hashedPassword = _passwordHasher.Hash(numericPassword);
            var verificationResult = _passwordHasher.Verify(numericPassword, hashedPassword);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(numericPassword);
            verificationResult.Should().BeTrue();
        }

        [Fact(DisplayName = "Should handle single character password correctly")]
        public void Should_Handle_Single_Character_Password_Correctly()
        {
            var singleCharPassword = "a";

            var hashedPassword = _passwordHasher.Hash(singleCharPassword);
            var verificationResult = _passwordHasher.Verify(singleCharPassword, hashedPassword);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(singleCharPassword);
            verificationResult.Should().BeTrue();
        }

        [Fact(DisplayName = "Should handle mixed case password correctly")]
        public void Should_Handle_Mixed_Case_Password_Correctly()
        {
            var mixedCasePassword = "MyPassword123!@#";

            var hashedPassword = _passwordHasher.Hash(mixedCasePassword);
            var verificationResult = _passwordHasher.Verify(mixedCasePassword, hashedPassword);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(mixedCasePassword);
            verificationResult.Should().BeTrue();
        }

        [Fact(DisplayName = "Should handle edge case with very short password")]
        public void Should_Handle_Edge_Case_With_Very_Short_Password()
        {
            var shortPassword = "ab";

            var hashedPassword = _passwordHasher.Hash(shortPassword);
            var verificationResult = _passwordHasher.Verify(shortPassword, hashedPassword);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(shortPassword);
            verificationResult.Should().BeTrue();
        }

        [Fact(DisplayName = "Should maintain consistency between hash and verify operations")]
        public void Should_Maintain_Consistency_Between_Hash_And_Verify_Operations()
        {
            var password = "ConsistentPassword123!";
            var hashedPassword = _passwordHasher.Hash(password);

            for (int i = 0; i < 10; i++)
            {
                var result = _passwordHasher.Verify(password, hashedPassword);
                result.Should().BeTrue($"Password verification should succeed on attempt {i + 1}");
            }
        }

        [Fact(DisplayName = "Should handle password with only special characters")]
        public void Should_Handle_Password_With_Only_Special_Characters()
        {
            var specialOnlyPassword = "!@#$%^&*()";

            var hashedPassword = _passwordHasher.Hash(specialOnlyPassword);
            var verificationResult = _passwordHasher.Verify(specialOnlyPassword, hashedPassword);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(specialOnlyPassword);
            verificationResult.Should().BeTrue();
        }

        [Fact(DisplayName = "Should handle password with spaces in the middle")]
        public void Should_Handle_Password_With_Spaces_In_The_Middle()
        {
            var passwordWithSpaces = "My Password 123";

            var hashedPassword = _passwordHasher.Hash(passwordWithSpaces);
            var verificationResult = _passwordHasher.Verify(passwordWithSpaces, hashedPassword);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Should().NotBe(passwordWithSpaces);
            verificationResult.Should().BeTrue();
        }

        [Fact(DisplayName = "Should throw ArgumentNullException when hashing null password")]
        public void Should_Throw_ArgumentNullException_When_Hashing_Null_Password()
        {
            string? nullPassword = null;

            var action = () => _passwordHasher.Hash(nullPassword!);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("plain");
        }

        [Fact(DisplayName = "Should throw ArgumentException when hashing empty password")]
        public void Should_Throw_ArgumentException_When_Hashing_Empty_Password()
        {
            var emptyPassword = "";

            var action = () => _passwordHasher.Hash(emptyPassword);

            action.Should().Throw<ArgumentException>()
                .WithMessage("Password cannot be empty or whitespace.*")
                .WithParameterName("plain");
        }

        [Fact(DisplayName = "Should throw ArgumentException when hashing whitespace password")]
        public void Should_Throw_ArgumentException_When_Hashing_Whitespace_Password()
        {
            var whitespacePassword = "   ";

            var action = () => _passwordHasher.Hash(whitespacePassword);

            action.Should().Throw<ArgumentException>()
                .WithMessage("Password cannot be empty or whitespace.*")
                .WithParameterName("plain");
        }

        [Fact(DisplayName = "Should throw ArgumentNullException when verifying with null password")]
        public void Should_Throw_ArgumentNullException_When_Verifying_With_Null_Password()
        {
            string? nullPassword = null;
            var hash = _passwordHasher.Hash("valid");

            var action = () => _passwordHasher.Verify(nullPassword!, hash);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("plainTextPassword");
        }

        [Fact(DisplayName = "Should throw ArgumentNullException when verifying with null hash")]
        public void Should_Throw_ArgumentNullException_When_Verifying_With_Null_Hash()
        {
            var password = "valid";
            string? nullHash = null;

            var action = () => _passwordHasher.Verify(password, nullHash!);

            action.Should().Throw<ArgumentNullException>()
                .WithParameterName("hash");
        }

        [Fact(DisplayName = "Should throw ArgumentException when verifying with empty password")]
        public void Should_Throw_ArgumentException_When_Verifying_With_Empty_Password()
        {
            var emptyPassword = "";
            var hash = _passwordHasher.Hash("valid");

            var action = () => _passwordHasher.Verify(emptyPassword, hash);

            action.Should().Throw<ArgumentException>()
                .WithMessage("Password cannot be empty or whitespace.*")
                .WithParameterName("plainTextPassword");
        }

        [Fact(DisplayName = "Should throw ArgumentException when verifying with empty hash")]
        public void Should_Throw_ArgumentException_When_Verifying_With_Empty_Hash()
        {
            var password = "valid";
            var emptyHash = "";

            var action = () => _passwordHasher.Verify(password, emptyHash);

            action.Should().Throw<ArgumentException>()
                .WithMessage("Hash cannot be empty or whitespace.*")
                .WithParameterName("hash");
        }
    }
}
