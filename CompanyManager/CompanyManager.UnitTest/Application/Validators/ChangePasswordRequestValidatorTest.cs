using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using FluentAssertions;

namespace CompanyManager.UnitTest.Application.Validators
{
    public sealed class ChangePasswordRequestValidatorTest
    {
        private static ChangePasswordRequest Valid() =>
            new()
            {
                Email = "john@acme.com",
                CurrentPassword = "CurrentPass123!",
                NewPassword = "NewStrongPass456!",
                ConfirmNewPassword = "NewStrongPass456!"
            };

        [Fact(DisplayName = "Should accept a fully valid request")]
        public void Should_Accept_Valid_Request()
        {
            var request = Valid();
            var validator = new ChangePasswordRequestValidator();

            var result = validator.Validate(request);

            result.IsValid.Should().BeTrue(result.ToString());
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
            var request = new ChangePasswordRequest
            {
                Email = email,
                CurrentPassword = "CurrentPass123!",
                NewPassword = "NewStrongPass456!",
                ConfirmNewPassword = "NewStrongPass456!"
            };
            var validator = new ChangePasswordRequestValidator();

            var result = validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.Email));
        }

        [Theory(DisplayName = "Should accept valid emails")]
        [InlineData("john@acme.com")]
        [InlineData("jane.doe@company.org")]
        [InlineData("user123@test.co.uk")]
        [InlineData("admin@example.com")]
        public void Should_Accept_Valid_Emails(string email)
        {
            var request = new ChangePasswordRequest
            {
                Email = email,
                CurrentPassword = "CurrentPass123!",
                NewPassword = "NewStrongPass456!",
                ConfirmNewPassword = "NewStrongPass456!"
            };
            var validator = new ChangePasswordRequestValidator();

            var result = validator.Validate(request);

            result.IsValid.Should().BeTrue(result.ToString());
        }

        [Theory(DisplayName = "Should reject invalid current passwords")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Should_Reject_Invalid_Current_Passwords(string password)
        {
            var request = new ChangePasswordRequest
            {
                Email = "john@acme.com",
                CurrentPassword = password,
                NewPassword = "NewStrongPass456!",
                ConfirmNewPassword = "NewStrongPass456!"
            };
            var validator = new ChangePasswordRequestValidator();

            var result = validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.CurrentPassword));
        }

        [Theory(DisplayName = "Should reject weak new passwords")]
        [InlineData("")] // vazio
        [InlineData(" ")] // apenas espaço
        [InlineData("weak")] // muito curto
        [InlineData("weakpass")] // sem maiúscula, dígito e caractere especial
        [InlineData("WeakPass")] // sem dígito e caractere especial
        [InlineData("WeakPass1")] // sem caractere especial
        [InlineData("weakpass1!")] // sem maiúscula
        [InlineData("WEAKPASS1!")] // sem minúscula
        [InlineData("WeakPass!")] // sem dígito
        public void Should_Reject_Weak_New_Passwords(string password)
        {
            var request = new ChangePasswordRequest
            {
                Email = "john@acme.com",
                CurrentPassword = "CurrentPass123!",
                NewPassword = password,
                ConfirmNewPassword = password
            };
            var validator = new ChangePasswordRequestValidator();

            var result = validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
        }

        [Theory(DisplayName = "Should accept strong new passwords")]
        [InlineData("StrongPass1!")]
        [InlineData("MySecure123@")]
        [InlineData("Complex#456")]
        [InlineData("VeryLongPassword789$")]
        [InlineData("P@ssw0rd!")]
        public void Should_Accept_Strong_New_Passwords(string password)
        {
            var request = new ChangePasswordRequest
            {
                Email = "john@acme.com",
                CurrentPassword = "CurrentPass123!",
                NewPassword = password,
                ConfirmNewPassword = password
            };
            var validator = new ChangePasswordRequestValidator();

            var result = validator.Validate(request);

            result.IsValid.Should().BeTrue(result.ToString());
        }

        [Theory(DisplayName = "Should reject password confirmation mismatches")]
        [InlineData("StrongPass1!", "StrongPass2!")]
        [InlineData("MySecure123@", "MySecure123#")]
        [InlineData("Complex#456", "Complex#789")]
        [InlineData("VeryLongPassword789$", "VeryLongPassword789%")]
        public void Should_Reject_Password_Confirmation_Mismatches(string newPassword, string confirmPassword)
        {
            var request = new ChangePasswordRequest
            {
                Email = "john@acme.com",
                CurrentPassword = "CurrentPass123!",
                NewPassword = newPassword,
                ConfirmNewPassword = confirmPassword
            };
            var validator = new ChangePasswordRequestValidator();

            var result = validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.ConfirmNewPassword));
        }

        [Theory(DisplayName = "Should reject invalid password confirmations")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void Should_Reject_Invalid_Password_Confirmations(string confirmPassword)
        {
            var request = new ChangePasswordRequest
            {
                Email = "john@acme.com",
                CurrentPassword = "CurrentPass123!",
                NewPassword = "ValidStrongPass123!",
                ConfirmNewPassword = confirmPassword
            };
            var validator = new ChangePasswordRequestValidator();

            var result = validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(ChangePasswordRequest.ConfirmNewPassword));
        }

        [Theory(DisplayName = "Should trim fields (no false negatives)")]
        [InlineData("  john@acme.com  ", "  CurrentPass123!  ", "  NewStrongPass456!  ", "  NewStrongPass456!  ")]
        [InlineData("  jane@company.org  ", "  OldPass789@  ", "  NewSecurePass123#  ", "  NewSecurePass123#  ")]
        public void Should_Trim_Fields(string email, string currentPassword, string newPassword, string confirmPassword)
        {
            var request = new ChangePasswordRequest
            {
                Email = email,
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = confirmPassword
            };

            var validator = new ChangePasswordRequestValidator();
            var result = validator.Validate(request);

            result.IsValid.Should().BeTrue(result.ToString());
        }

        [Fact(DisplayName = "Should provide clear error messages for validation failures")]
        public void Should_Provide_Clear_Error_Messages()
        {
            var request = new ChangePasswordRequest
            {
                Email = "",
                CurrentPassword = "",
                NewPassword = "weak",
                ConfirmNewPassword = "different"
            };

            var validator = new ChangePasswordRequestValidator();
            var result = validator.Validate(request);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(4);
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Email is required"));
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Current password is required"));
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("New password must be at least 8 characters"));
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Password confirmation does not match"));
        }
    }
}
