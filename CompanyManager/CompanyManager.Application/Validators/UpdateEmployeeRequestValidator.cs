using System;
using CompanyManager.Application.DTOs;
using CompanyManager.Domain.ValueObjects;
using FluentValidation;
using System.Linq;

namespace CompanyManager.Application.Validators
{
    public sealed class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
    {
        public UpdateEmployeeRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("First name is required.")
                .Must(HasNonWhitespaceContent).WithMessage("First name is required.")
                .Must(s => s!.Trim().Length >= 2).WithMessage("First name must have at least 2 characters.");

            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Last name is required.")
                .Must(HasNonWhitespaceContent).WithMessage("Last name is required.")
                .Must(s => s!.Trim().Length >= 2).WithMessage("Last name must have at least 2 characters.");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required.")
                .Must(IsValidEmail).WithMessage("Invalid email.");

            RuleFor(x => x.DocumentNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Document number is required.")
                .Must(IsValidCpf).WithMessage("Invalid CPF.");

            RuleFor(x => x.PhoneNumbers)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("At least one phone number is required.")
                .Must(phones => phones != null && phones.All(IsValidBrPhone))
                .WithMessage("Invalid phone number.");

            RuleFor(x => x.JobTitleId)
                .Must(id => id != Guid.Empty)
                .WithMessage("Job title is required.");

            RuleFor(x => x.DepartmentId)
                .Must(id => id != Guid.Empty).WithMessage("DepartmentId is required.");

            // RoleLevel removido - o nível é determinado pelo JobTitle.HierarchyLevel

            RuleFor(x => x.Password)
                .Must(password => string.IsNullOrEmpty(password) || password.Length >= 6)
                .WithMessage("Password must have at least 6 characters if provided.");
        }

        private static bool HasNonWhitespaceContent(string? value) =>
            !string.IsNullOrWhiteSpace(value?.Trim());

        private static bool IsValidEmail(string? value)
        {
            var input = value?.Trim();
            try { _ = new Email(input ?? string.Empty); return true; }
            catch { return false; }
        }

        private static bool IsValidCpf(string? value)
        {
            var input = value?.Trim();
            try { _ = new DocumentNumber(input ?? string.Empty); return true; }
            catch { return false; }
        }


        private static bool IsValidBrPhone(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var input = value.Trim();

            try 
            { 
                _ = new PhoneNumber(input, defaultCountry: "BR"); 
                return true; 
            }
            catch 
            { 
                return false; 
            }
        }
    }
}
