using FluentAssertions;
using CompanyManager.Domain.ValueObjects;
using System;

namespace CompanyManager.UnitTest.ValueObjects
{
    public class DocumentNumberTest
    {
        [Fact(DisplayName = "Should create valid CPF (digits-only) successfully")]
        public void Should_Create_Valid_CPF_DigitsOnly_Successfully()
        {
            var validCpf = "52998224725";

            var doc = new DocumentNumber(validCpf);

            doc.Should().NotBeNull();
            doc.Raw.Should().Be(validCpf);
            doc.Digits.Should().Be("52998224725");
        }

        [Fact(DisplayName = "Should create valid CPF (masked) successfully")]
        public void Should_Create_Valid_CPF_Masked_Successfully()
        {
            var validCpfMasked = "111.444.777-35";

            var doc = new DocumentNumber(validCpfMasked);

            doc.Raw.Should().Be(validCpfMasked);
            doc.Digits.Should().Be("11144477735");
        }

        [Theory(DisplayName = "Should accept valid CPF formats (with check digits)")]
        [InlineData("52998224725")]
        [InlineData("111.444.777-35")]
        public void Should_Accept_Valid_CPF_Formats(string input)
        {
            var doc = new DocumentNumber(input);

            doc.Raw.Should().Be(input);
        }

        [Fact(DisplayName = "Should trim spaces before and after")]
        public void Should_Trim_Spaces_Before_And_After()
        {
            var input = "   111.444.777-35   ";

            var doc = new DocumentNumber(input);

            doc.Raw.Should().Be("111.444.777-35");
            doc.Digits.Should().Be("11144477735");
        }

        [Theory(DisplayName = "Should trim spaces for valid CPF formats")]
        [InlineData("  52998224725  ", "52998224725")]
        [InlineData(" \t 111.444.777-35 \n ", "111.444.777-35")]
        public void Should_Trim_Spaces_For_Valid_CPF_Formats(string input, string expectedRaw)
        {
            var doc = new DocumentNumber(input);
            doc.Raw.Should().Be(expectedRaw);
        }

        [Fact(DisplayName = "Should throw for null document number")]
        public void Should_Throw_For_Null()
        {
            string? input = null;

            var ex = Assert.Throws<ArgumentException>(() => new DocumentNumber(input!));
            ex.Message.Should().Contain("Document number cannot be null or empty");
        }

        [Theory(DisplayName = "Should throw for empty or whitespace")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData(" \t \n ")]
        public void Should_Throw_For_Empty_Or_Whitespace(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new DocumentNumber(input));
            ex.Message.Should().Contain("Document number cannot be null or empty");
        }

        [Theory(DisplayName = "Should throw for invalid formats (not a valid CPF)")]
        [InlineData("123")]
        [InlineData("123456789")]
        [InlineData("123456789012345678901")]
        [InlineData("abc123def")]
        [InlineData("123-456-789")]
        [InlineData("123.456.789")]
        [InlineData("123/456/789")]
        public void Should_Throw_For_Invalid_Formats(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new DocumentNumber(input));
            ex.Message.Should().Contain("Invalid document number format");
        }

        [Theory(DisplayName = "Should throw for CPF with invalid check digits (DV)")]
        [InlineData("52998224724")]
        [InlineData("111.444.777-34")]
        [InlineData("12345678900")]
        public void Should_Throw_For_CPF_With_Invalid_Check_Digits(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new DocumentNumber(input));
            ex.Message.Should().Contain("Invalid document number format");
        }

        [Fact(DisplayName = "Should implement equality by value")]
        public void Should_Implement_Equality()
        {
            var plain = new DocumentNumber("52998224725");
            var masked = new DocumentNumber("529.982.247-25");

            plain.Should().Be(masked);
            plain.GetHashCode().Should().Be(masked.GetHashCode());
        }

        [Fact(DisplayName = "Should preserve original format in Raw property")]
        public void Should_Preserve_Original_Format_In_Raw_Property()
        {
            var plain = new DocumentNumber("52998224725");
            var masked = new DocumentNumber("111.444.777-35");

            plain.ToString().Should().Be("52998224725");
            plain.Formatted.Should().Be("529.982.247-25");

            masked.ToString().Should().Be("111.444.777-35");
            masked.Formatted.Should().Be("111.444.777-35");
        }
    }
}
