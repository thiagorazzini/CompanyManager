using CompanyManager.Application.DTOs;
using CompanyManager.Domain.ValueObjects;
// using CompanyManager.Domain.AccessControl; - removido, não é mais necessário
using FluentValidation;
using System.Globalization;
using System;

namespace CompanyManager.Application.Validators;


public sealed class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    private const string DateFormat = "yyyy-MM-dd";

    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("First name is required.")
            .Must(text => HasMinTrimmedLength(text, 2))
                .WithMessage("First name must have at least 2 characters.");

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Last name is required.")
            .Must(text => HasMinTrimmedLength(text, 2))
                .WithMessage("Last name must have at least 2 characters.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .Must(IsValidEmailFormat).WithMessage("Invalid email.");

        RuleFor(x => x.DocumentNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Document number is required.")
            .Must(IsValidCpfNumber).WithMessage("Invalid CPF.");

        RuleFor(x => x.PhoneNumbers)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("At least one phone number is required.");

        RuleForEach(x => x.PhoneNumbers)
            .Must(IsValidBrazilianPhoneNumber).WithMessage("Invalid phone number.");

        RuleFor(x => x.DateOfBirth)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Date of birth is required.")
            .Must(IsValidDateInYyyyMmDdFormat).WithMessage($"Date of birth must be {DateFormat}.")
            .Must(IsNotInFuture).WithMessage("Date of birth cannot be in the future.")
            .Must(IsAtLeast18YearsOld).WithMessage("Employee must be at least 18 years old.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[a-z]").WithMessage("Password must have a lowercase letter.")
            .Matches("[A-Z]").WithMessage("Password must have an uppercase letter.")
            .Matches(@"\d").WithMessage("Password must have a digit.");

        RuleFor(x => x.JobTitleId)
            .Must(id => id != Guid.Empty).WithMessage("Job title is required.");

        RuleFor(x => x.DepartmentId)
            .Must(id => id != Guid.Empty).WithMessage("DepartmentId is required.");

        // RoleLevel removido - o nível é determinado pelo JobTitle.HierarchyLevel
    }

    // ------------ Helpers (Clean Code) ------------

    private static bool HasMinTrimmedLength(string? text, int minLength)
    {
        try
        {
            var trimmed = text?.Trim() ?? string.Empty;
            return trimmed.Length >= minLength;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidEmailFormat(string? emailText)
    {
        try
        {
            var normalized = emailText?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
                return false;
                
            var email = new Email(normalized);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsValidCpfNumber(string? documentText)
    {
        try
        {
            var normalized = documentText?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
                return false;
                
            var document = new DocumentNumber(normalized);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsValidBrazilianPhoneNumber(string? phoneText)
    {
        try
        {
            var normalized = phoneText?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
                return false;
                
            var phone = new PhoneNumber(normalized, defaultCountry: "BR");
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsValidDateInYyyyMmDdFormat(string? dateText)
    {
        try
        {
            return TryParseDateYyyyMmDd(dateText, out _);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsNotInFuture(string? dateText)
    {
        try
        {
            return TryParseDateYyyyMmDd(dateText, out var date) && date <= DateTime.Today;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAtLeast18YearsOld(string? dateText)
    {
        try
        {
            if (!TryParseDateYyyyMmDd(dateText, out var birthDate))
                return false;

            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            
            // Se ainda não fez aniversário este ano, subtrai 1
            if (birthDate.Date > today.AddYears(-age))
                age--;
                
            return age >= 18;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseDateYyyyMmDd(string? dateText, out DateTime date)
    {
        try
        {
            var normalized = dateText?.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                date = default;
                return false;
            }

            return DateTime.TryParseExact(
                normalized,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }
        catch
        {
            date = default;
            return false;
        }
    }

    // IsValidRoleLevel removido - não é mais necessário
}
