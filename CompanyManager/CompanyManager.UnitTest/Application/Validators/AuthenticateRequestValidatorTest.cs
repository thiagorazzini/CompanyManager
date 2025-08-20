using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using FluentAssertions;

namespace CompanyManager.UnitTest.Application.Validators
{
    public sealed class AuthenticateRequestValidatorTest
    {
        private static AuthenticateRequest Valid()
        {
            return new AuthenticateRequest
            {
                Email = "john@acme.com",
                Password = "password123"
            };
        }

        [Fact(DisplayName = "Should accept a fully valid request")]
        public void Should_Accept_Valid_Request()
        {
            var req = Valid();

            var v = new AuthenticateRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Theory(DisplayName = "Should reject invalid emails")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid-email")]
        [InlineData("@acme.com")]
        [InlineData("john@")]
        [InlineData("john.acme.com")]
        public void Should_Reject_Invalid_Emails(string email)
        {
            var req = new AuthenticateRequest { Email = email, Password = "password123" };

            var v = new AuthenticateRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(AuthenticateRequest.Email));
        }

        [Theory(DisplayName = "Should accept valid emails")]
        [InlineData("john@acme.com")]
        [InlineData("jane.doe@company.org")]
        [InlineData("user123@test.co.uk")]
        [InlineData("admin@example.com")]
        public void Should_Accept_Valid_Emails(string email)
        {
            var req = new AuthenticateRequest { Email = email, Password = "password123" };

            var v = new AuthenticateRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Theory(DisplayName = "Should reject invalid passwords")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("12345")] // menos de 6 caracteres
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Should_Reject_Invalid_Passwords(string password)
        {
            var req = new AuthenticateRequest { Email = "john@acme.com", Password = password };

            var v = new AuthenticateRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(AuthenticateRequest.Password));
        }

        [Theory(DisplayName = "Should accept valid passwords")]
        [InlineData("password123")]
        [InlineData("123456")]
        [InlineData("P@ssw0rd")]
        [InlineData("verylongpassword")]
        public void Should_Accept_Valid_Passwords(string password)
        {
            var req = new AuthenticateRequest { Email = "john@acme.com", Password = password };

            var v = new AuthenticateRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Theory(DisplayName = "Should trim fields (no false negatives)")]
        [InlineData("  john@acme.com  ", "  password123  ")]
        [InlineData("  jane@company.org  ", "  secret456  ")]
        public void Should_Trim_Fields(string email, string password)
        {
            var req = new AuthenticateRequest { Email = email, Password = password };

            var v = new AuthenticateRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }
    }
}
