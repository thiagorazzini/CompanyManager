using FluentAssertions;
using CompanyManager.Domain.ValueObjects;
using System;

namespace CompanyManager.UnitTest.ValueObjects
{
    public class EmailTest
    {

        [Theory(DisplayName = "Should create valid emails and normalize to lowercase")]
        [InlineData("user@example.com", "user@example.com")]
        [InlineData("USER@EXAMPLE.COM", "user@example.com")]
        [InlineData("User.Name+Tag@Sub.Domain.COM", "user.name+tag@sub.domain.com")]
        [InlineData("joao@exâmple.com", "joao@exâmple.com")] // IDN (Unicode) -> apenas lowercase
        public void Should_Create_And_Normalize(string input, string expected)
        {
            var email = new Email(input);
            email.Value.Should().Be(expected);
        }

        [Theory(DisplayName = "Should trim borders then normalize")]
        [InlineData("  USER@EXAMPLE.COM  ", "user@example.com")]
        [InlineData("\tUser.Name+Tag@Sub.Domain.Com \n", "user.name+tag@sub.domain.com")]
        public void Should_Trim_And_Normalize(string input, string expected)
        {
            var email = new Email(input);
            email.Value.Should().Be(expected);
        }


        [Fact(DisplayName = "Should throw for null")]
        public void Should_Throw_For_Null()
        {
            string? input = null;
            var ex = Assert.Throws<ArgumentException>(() => new Email(input!));
            ex.Message.Should().Contain("Email cannot be null or empty");
        }

        [Theory(DisplayName = "Should throw for empty or whitespace")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData(" \t \n ")]
        public void Should_Throw_For_Empty_Or_Whitespace(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new Email(input));
            ex.Message.Should().Contain("Email cannot be null or empty");
        }


        [Theory(DisplayName = "Should throw for missing or multiple @")]
        [InlineData("userexample.com")]            // no @
        [InlineData("user@@example.com")]          // two @
        [InlineData("user@example@com")]           // two @
        public void Should_Throw_For_Bad_At_Sign_Usage(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new Email(input));
            ex.Message.Should().Contain("Invalid email format");
        }

        [Theory(DisplayName = "Should throw for inner whitespace")]
        [InlineData("user @example.com")]
        [InlineData("user@ example.com")]
        [InlineData("user@exam ple.com")]
        public void Should_Throw_For_Inner_Whitespace(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new Email(input));
            ex.Message.Should().Contain("Invalid email format");
        }

        [Theory(DisplayName = "Should throw for display name or angle brackets")]
        [InlineData("\"John\" <john@example.com>")]
        [InlineData("<john@example.com>")]
        [InlineData("John <john@example.com>")]
        public void Should_Throw_For_DisplayName(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new Email(input));
            ex.Message.Should().Contain("Invalid email format");
        }


        [Theory(DisplayName = "Should accept common local-part punctuation")]
        [InlineData("user.name+tag@example.com")]
        [InlineData("user_name-tag@example.com")]
        [InlineData("u!#$%&'*+-/=?^_`{|}~r@example.com")]
        public void Should_Accept_Common_LocalPart_Punctuation(string input)
        {
            var email = new Email(input);
            email.Value.Should().Be(input.ToLowerInvariant());
        }

        [Theory(DisplayName = "Should throw for invalid local-part layout")]
        [InlineData(".user@example.com")]    // leading dot
        [InlineData("user.@example.com")]    // trailing dot
        [InlineData("user..name@example.com")] // double dot
        public void Should_Throw_For_Invalid_LocalPart_Layout(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new Email(input));
            ex.Message.Should().Contain("Invalid email format");
        }

        [Fact(DisplayName = "Should throw when local-part exceeds 64 chars")]
        public void Should_Throw_For_LocalPart_Too_Long()
        {
            var local = new string('a', 65);
            var input = $"{local}@example.com";
            var ex = Assert.Throws<ArgumentException>(() => new Email(input));
            ex.Message.Should().Contain("Invalid email format");
        }


        [Theory(DisplayName = "Should throw for domain without dot or with leading/trailing dot or double dots")]
        [InlineData("user@localhost")]          // no dot
        [InlineData("user@.example.com")]       // leading dot
        [InlineData("user@example.com.")]       // trailing dot
        [InlineData("user@exa..mple.com")]      // double dot
        public void Should_Throw_For_Bad_Domain_Dots(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new Email(input));
            ex.Message.Should().Contain("Invalid email format");
        }

        [Theory(DisplayName = "Should throw for invalid domain labels")]
        [InlineData("user@-example.com")]       // label starts with hyphen
        [InlineData("user@example-.com")]       // label ends with hyphen
        [InlineData("user@exam!ple.com")]       // invalid char in label
        public void Should_Throw_For_Invalid_Domain_Labels(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new Email(input));
            ex.Message.Should().Contain("Invalid email format");
        }

        [Fact(DisplayName = "Should throw when a domain label exceeds 63 chars")]
        public void Should_Throw_For_Label_Too_Long()
        {
            var label = new string('a', 64); // > 63
            var input = $"user@{label}.com";
            var ex = Assert.Throws<ArgumentException>(() => new Email(input));
            ex.Message.Should().Contain("Invalid email format");
        }

        [Fact(DisplayName = "Should throw when total length exceeds 254 chars")]
        public void Should_Throw_For_Total_Length_Too_Long()
        {
            var local = "user";
            var longDomain = string.Join(".", Enumerable.Repeat("subdomain", 25)); // faz passar 254 fácil
            var input = $"{local}@{longDomain}.com";
            input.Length.Should().BeGreaterThan(254);

            var ex = Assert.Throws<ArgumentException>(() => new Email(input));
            ex.Message.Should().Contain("Invalid email format");
        }


        [Fact(DisplayName = "Should implement value equality (case-insensitive normalization)")]
        public void Should_Implement_Value_Equality()
        {
            var a = new Email("USER@EXAMPLE.COM");
            var b = new Email("user@example.com");
            a.Should().Be(b);
            a.Equals(b).Should().BeTrue();
            a.GetHashCode().Should().Be(b.GetHashCode());
        }

        [Fact(DisplayName = "Should implement ToString as normalized Value")]
        public void Should_ToString_Return_Value()
        {
            var e = new Email("User.Name+Tag@Sub.Domain.Com");
            e.ToString().Should().Be("user.name+tag@sub.domain.com");
        }
    }
}
