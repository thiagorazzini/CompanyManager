using CompanyManager.Application.DTOs;
using FluentValidation;
using System;

namespace CompanyManager.Application.Validators
{
    public sealed class UpdateDepartmentRequestValidator : AbstractValidator<UpdateDepartmentRequest>
    {
        public UpdateDepartmentRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Department ID is required.");

            RuleFor(x => x.NewName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Department name is required.")
                .Must(HasNonWhitespaceContent).WithMessage("Department name is required.")
                .Must(s => s!.Trim().Length >= 2).WithMessage("Department name must have at least 2 characters.");
        }

        private static bool HasNonWhitespaceContent(string? value) =>
            !string.IsNullOrWhiteSpace(value?.Trim());
    }
}
