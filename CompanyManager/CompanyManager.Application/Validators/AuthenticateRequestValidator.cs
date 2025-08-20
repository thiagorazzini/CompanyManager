using CompanyManager.Application.DTOs;
using FluentValidation;

namespace CompanyManager.Application.Validators
{
    public sealed class AuthenticateRequestValidator : AbstractValidator<AuthenticateRequest>
    {
        public AuthenticateRequestValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required.")
                .Must(HasNonWhitespaceContent).WithMessage("Email is required.")
                .Must(IsValidEmailFormat).WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required.")
                .Must(HasNonWhitespaceContent).WithMessage("Password is required.")
                .Must(p => p!.Length >= 6).WithMessage("Password must have at least 6 characters.");
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
    }
}
