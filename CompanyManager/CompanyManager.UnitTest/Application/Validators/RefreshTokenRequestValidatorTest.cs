using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using FluentAssertions;

namespace CompanyManager.UnitTest.Application.Validators
{
    public sealed class RefreshTokenRequestValidatorTest
    {
        private static RefreshTokenRequest Valid()
        {
            return new RefreshTokenRequest
            {
                RefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
            };
        }

        [Fact(DisplayName = "Should accept a fully valid request")]
        public void Should_Accept_Valid_Request()
        {
            var req = Valid();

            var v = new RefreshTokenRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Theory(DisplayName = "Should reject invalid refresh tokens")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid-token")]
        [InlineData("part1.part2")] // apenas 2 partes
        [InlineData("part1.part2.part3.part4")] // 4 partes
        [InlineData("..")] // partes vazias
        [InlineData("part1..part3")] // parte do meio vazia
        [InlineData(".part2.part3")] // primeira parte vazia
        [InlineData("part1.part2.")] // última parte vazia
        public void Should_Reject_Invalid_Tokens(string token)
        {
            var req = new RefreshTokenRequest { RefreshToken = token };

            var v = new RefreshTokenRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(RefreshTokenRequest.RefreshToken));
        }

        [Theory(DisplayName = "Should accept valid JWT tokens")]
        [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c")]
        [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXN0IiwiaWF0IjoxNjM0NTY3ODkwfQ.test-signature")]
        [InlineData("header.payload.signature")]
        [InlineData("a.b.c")]
        public void Should_Accept_Valid_Tokens(string token)
        {
            var req = new RefreshTokenRequest { RefreshToken = token };

            var v = new RefreshTokenRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Theory(DisplayName = "Should trim fields (no false negatives)")]
        [InlineData("  eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c  ")]
        [InlineData("  header.payload.signature  ")]
        public void Should_Trim_Fields(string token)
        {
            var req = new RefreshTokenRequest { RefreshToken = token };

            var v = new RefreshTokenRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }
    }
}
