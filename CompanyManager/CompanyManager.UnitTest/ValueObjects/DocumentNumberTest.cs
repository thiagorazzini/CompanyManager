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
            var validCpf = "52998224725"; // válido

            var doc = new DocumentNumber(validCpf);

            doc.Should().NotBeNull();
            doc.Raw.Should().Be(validCpf);        // preserva entrada
            doc.Digits.Should().Be("52998224725"); // canonical
        }

        [Fact(DisplayName = "Should create valid CPF (masked) successfully")]
        public void Should_Create_Valid_CPF_Masked_Successfully()
        {
            var validCpfMasked = "111.444.777-35"; // válido

            var doc = new DocumentNumber(validCpfMasked);

            doc.Raw.Should().Be(validCpfMasked);     // preserva máscara
            doc.Digits.Should().Be("11144477735");   // canonical
        }

        [Theory(DisplayName = "Should accept valid CPF formats (with check digits)")]
        [InlineData("52998224725")]     // sem máscara
        [InlineData("111.444.777-35")]  // com máscara
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

            doc.Raw.Should().Be("111.444.777-35");   // bordas removidas
            doc.Digits.Should().Be("11144477735");   // canonical
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
        [InlineData("123")]                   // curto
        [InlineData("123456789")]             // tamanho errado
        [InlineData("123456789012345678901")] // longo demais
        [InlineData("abc123def")]             // letras
        [InlineData("123-456-789")]           // pontuação inválida
        [InlineData("123.456.789")]           // incompleto
        [InlineData("123/456/789")]           // pontuação inválida
        public void Should_Throw_For_Invalid_Formats(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new DocumentNumber(input));
            ex.Message.Should().Contain("Invalid document number format");
        }

        [Theory(DisplayName = "Should throw for CPF with invalid check digits (DV)")]
        [InlineData("52998224724")]        // um dígito alterado
        [InlineData("111.444.777-34")]     // máscara ok, DV inválido
        [InlineData("12345678900")]        // 11 dígitos mas DV inválido
        public void Should_Throw_For_CPF_InvalidDV(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new DocumentNumber(input));
            ex.Message.Should().Contain("Invalid document number format");
        }

        [Theory(DisplayName = "Should throw for repeated-digit CPFs (e.g., 000… / 111… / 222…)")]
        [InlineData("00000000000")]
        [InlineData("11111111111")]
        [InlineData("22222222222")]
        [InlineData("33333333333")]
        [InlineData("44444444444")]
        [InlineData("55555555555")]
        [InlineData("66666666666")]
        [InlineData("77777777777")]
        [InlineData("88888888888")]
        [InlineData("99999999999")]
        public void Should_Throw_For_Repeated_Digit_CPFs(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new DocumentNumber(input));
            ex.Message.Should().Contain("Invalid document number format");
        }

        [Fact(DisplayName = "Should implement equality by digits-only (masked vs non-masked must be equal)")]
        public void Should_Implement_Equality_By_Digits()
        {
            var masked = new DocumentNumber("111.444.777-35");
            var plain = new DocumentNumber("11144477735");

            masked.Should().Be(plain);          // igualdade por Digits
            masked.Equals(plain).Should().BeTrue();
            masked.GetHashCode().Should().Be(plain.GetHashCode());
        }

        [Fact(DisplayName = "Should implement ToString as Raw and expose Formatted")]
        public void Should_ToString_Raw_And_Formatted_From_Digits()
        {
            var plain = new DocumentNumber("52998224725");      // sem máscara
            var masked = new DocumentNumber("111.444.777-35");  // com máscara

            plain.ToString().Should().Be("52998224725");        // Raw
            plain.Formatted.Should().Be("529.982.247-25");      // máscara gerada a partir de Digits

            masked.ToString().Should().Be("111.444.777-35");    // Raw preservada
            masked.Formatted.Should().Be("111.444.777-35");     // já estava formatado
        }
    }
}
