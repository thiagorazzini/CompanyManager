using CompanyManager.Application.DTOs;
using FluentValidation;

namespace CompanyManager.Application.Validators
{
    public sealed class CreateDepartmentRequestValidator : AbstractValidator<CreateDepartmentRequest>
    {
        public CreateDepartmentRequestValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Department name is required.")
                .Must(HasNonWhitespaceContent).WithMessage("Department name is required.")
                .Must(s => s!.Trim().Length >= 2).WithMessage("Department name must have at least 2 characters.");
        }

        private static bool HasNonWhitespaceContent(string? value) =>
            !string.IsNullOrWhiteSpace(value?.Trim());
    }
}
