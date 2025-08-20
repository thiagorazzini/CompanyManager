using CompanyManager.Application.DTOs;
using FluentValidation;
using System.Text.RegularExpressions;

namespace CompanyManager.Application.Validators
{
    public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        private static readonly Regex LowercaseRegex = new(@"[a-z]", RegexOptions.Compiled);
        private static readonly Regex UppercaseRegex = new(@"[A-Z]", RegexOptions.Compiled);
        private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);
        private static readonly Regex SpecialCharRegex = new(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]", RegexOptions.Compiled);

        public ChangePasswordRequestValidator()
        {
            ConfigureEmailValidation();
            ConfigureCurrentPasswordValidation();
            ConfigureNewPasswordValidation();
            ConfigureConfirmPasswordValidation();
        }

        private void ConfigureEmailValidation()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required.")
                .Must(HasNonWhitespaceContent).WithMessage("Email is required.")
                .Must(IsValidEmailFormat).WithMessage("Invalid email format.");
        }

        private void ConfigureCurrentPasswordValidation()
        {
            RuleFor(x => x.CurrentPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Current password is required.")
                .Must(HasNonWhitespaceContent).WithMessage("Current password is required.");
        }

        private void ConfigureNewPasswordValidation()
        {
            RuleFor(x => x.NewPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("New password is required.")
                .Must(HasNonWhitespaceContent).WithMessage("New password is required.")
                .Must(IsStrongPassword).WithMessage("New password must be at least 8 characters long and contain at least one lowercase letter, one uppercase letter, one digit, and one special character.");
        }

        private void ConfigureConfirmPasswordValidation()
        {
            RuleFor(x => x.ConfirmNewPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password confirmation is required.")
                .Must(HasNonWhitespaceContent).WithMessage("Password confirmation is required.")
                .Must((request, confirmPassword) => PasswordsMatch(request.NewPassword, confirmPassword))
                .WithMessage("Password confirmation does not match the new password.");
        }

        private static bool HasNonWhitespaceContent(string? value) =>
            !string.IsNullOrWhiteSpace(value?.Trim());

        private static bool IsValidEmailFormat(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            
            try
            {
                var email = new CompanyManager.Domain.ValueObjects.Email(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsStrongPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            var trimmedPassword = password.Trim();
            
            return trimmedPassword.Length >= 8 &&
                   LowercaseRegex.IsMatch(trimmedPassword) &&
                   UppercaseRegex.IsMatch(trimmedPassword) &&
                   DigitRegex.IsMatch(trimmedPassword) &&
                   SpecialCharRegex.IsMatch(trimmedPassword);
        }

        private static bool PasswordsMatch(string newPassword, string confirmPassword) =>
            string.Equals(newPassword?.Trim(), confirmPassword?.Trim(), StringComparison.Ordinal);
    }
}
