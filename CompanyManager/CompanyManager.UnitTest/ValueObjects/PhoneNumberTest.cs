using FluentAssertions;
using CompanyManager.Domain.ValueObjects;
using System;
using System.Linq;

namespace CompanyManager.UnitTest.ValueObjects
{
    public class PhoneNumberTest
    {

        [Theory(DisplayName = "Should canonicalize to BR ddi (BR default country)")]
        [InlineData("(11) 99999-9999", "+5511999999999", "55", "11999999999")] // m�vel BR (11 d�gitos N(S)N)
        [InlineData("11 99999-9999", "+5511999999999", "55", "11999999999")]
        [InlineData("11999999999", "+5511999999999", "55", "11999999999")]
        public void Should_Canonicalize_To_E164_BR(string input, string expectedE164, string expectedCc, string expectedNsN)
        {
            var p = new PhoneNumber(input, defaultCountry: "BR");
            p.E164.Should().Be(expectedE164);
            p.CountryCode.Should().Be(expectedCc);
            p.NationalNumber.Should().Be(expectedNsN);
            p.Raw.Should().Be(input.Trim());
        }

        [Theory(DisplayName = "Should keep already E.164 numbers unchanged")]
        [InlineData("+5511999999999", "+5511999999999", "55", "11999999999")]
        [InlineData("+14155552671", "+14155552671", "1", "4155552671")] // US sample
        public void Should_Keep_E164(string input, string expectedE164, string expectedCc, string expectedNsN)
        {
            var p = new PhoneNumber(input);
            p.E164.Should().Be(expectedE164);
            p.CountryCode.Should().Be(expectedCc);
            p.NationalNumber.Should().Be(expectedNsN);
        }


        [Theory(DisplayName = "Should trim borders and ignore formatting characters")]
        [InlineData("  +55 11 99999-9999  ", "+5511999999999")]
        [InlineData("\t(11) 99999-9999\n", "+5511999999999")]
        public void Should_Trim_And_Sanitize(string input, string expectedE164)
        {
            var p = new PhoneNumber(input, defaultCountry: "BR");
            p.E164.Should().Be(expectedE164);
        }



        [Theory(DisplayName = "Should validate BR mobile (9 digits starting with 9)")]
        [InlineData("11 99999-9999")] // ok
        public void Should_Validate_BR_Mobile(string input)
        {
            var p = new PhoneNumber(input, defaultCountry: "BR");
            p.E164.Should().Be("+5511999999999");
        }

        [Theory(DisplayName = "Should throw for invalid BR mobile/fixed layouts")]
        [InlineData("11 89999-9999")]     
        [InlineData("00 99999-9999")]      
        [InlineData("11 01234-5678")]      
        public void Should_Throw_For_Invalid_BR_Layouts(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new PhoneNumber(input, defaultCountry: "BR"));
            ex.Message.Should().Contain("Invalid phone number format");
        }


        [Theory(DisplayName = "Should reject too short/too long numbers (generic 8..15 digits)")]
        [InlineData("+55 11 999", "Invalid phone number format")] // curto
        [InlineData("+999 1234567890123456", "Invalid phone number format")] // >15 d�gitos
        public void Should_Reject_Length_Out_Of_Bounds(string input, string partMessage)
        {
            var ex = Assert.Throws<ArgumentException>(() => new PhoneNumber(input));
            ex.Message.Should().Contain(partMessage);
        }


        [Theory(DisplayName = "Should capture extension without affecting E.164")]
        [InlineData("+55 11 99999-9999 x123", "+5511999999999", "123")]
        [InlineData("(11) 99999-9999 ext.456", "+5511999999999", "456")]
        [InlineData("11 99999-9999 ramal 789", "+5511999999999", "789")]
        public void Should_Capture_Extension(string input, string expectedE164, string expectedExt)
        {
            var p = new PhoneNumber(input, defaultCountry: "BR");
            p.E164.Should().Be(expectedE164);
            p.Extension.Should().Be(expectedExt);
        }


        [Theory(DisplayName = "Should throw for null/empty/whitespace")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Should_Throw_For_Null_Or_Whitespace(string? input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new PhoneNumber(input!));
            ex.Message.Should().Contain("Phone number cannot be null or empty");
        }

        [Theory(DisplayName = "Should throw for invalid characters or malformed input")]
        [InlineData("abc")]
        [InlineData("invalid-phone")]
        [InlineData("+55 +55 11 99999-9999")]
        [InlineData("+")]
        [InlineData("+++5511999999999")]
        public void Should_Throw_For_Invalid_Input(string input)
        {
            var ex = Assert.Throws<ArgumentException>(() => new PhoneNumber(input, defaultCountry: "BR"));
            ex.Message.Should().Contain("Invalid phone number format");
        }


        [Fact(DisplayName = "Should use E.164 for equality and hash (masked vs unmasked)")]
        public void Should_Use_E164_For_Equality()
        {
            var masked = new PhoneNumber("+55 11 99999-9999");
            var unmasked = new PhoneNumber("11999999999", defaultCountry: "BR");

            masked.Should().Be(unmasked);
            masked.Equals(unmasked).Should().BeTrue();
            masked.GetHashCode().Should().Be(unmasked.GetHashCode());
        }

        [Fact(DisplayName = "ToString should return E.164")]
        public void ToString_Should_Return_E164()
        {
            var p = new PhoneNumber("(11) 99999-9999", defaultCountry: "BR");
            p.ToString().Should().Be("+5511999999999");
        }

        [Fact(DisplayName = "Should expose masked representation for UI")]
        public void Should_Expose_Masked()
        {
            var p = new PhoneNumber("11987651234", defaultCountry: "BR");
            p.Masked.Should().StartWith("+55 11 9");         // mant�m DDI e DDD
            p.Masked.Should().Contain("XXXX");               // parte mascarada
            p.Masked.Should().EndWith("1234");               // �ltimos 4 d�gitos vis�veis
        }
    }
}
