using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;

namespace CompanyManager.UnitTest.Application.Validators
{
    public sealed class CreateEmployeeRequestValidatorTest
    {
        private const string DateFormat = "yyyy-MM-dd";
        private readonly CreateEmployeeRequestValidator _validator = new();

        // ---------- Test helpers ----------
        private static CreateEmployeeRequest BuildValidRequest() => new()
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@company.com",
            DocumentNumber = "52998224725",                // CPF válido
            PhoneNumbers = new List<string> { "11999999999" }, // BR
            JobTitle = "Developer",
            DateOfBirth = DateTime.Today.AddYears(-25).ToString(DateFormat),
            RoleLevel = "Junior",
            Password = "Strong123!",
            DepartmentId = Guid.NewGuid()
        };

        private FluentValidation.Results.ValidationResult Validate(Action<CreateEmployeeRequest>? mutate = null)
        {
            var req = BuildValidRequest();
            mutate?.Invoke(req);
            return _validator.Validate(req);
        }

        private static void ShouldHaveErrorFor(FluentValidation.Results.ValidationResult result, string propertyName, bool startsWith = false)
        {
            if (startsWith)
                result.Errors.Should().Contain(e => e.PropertyName.StartsWith(propertyName));
            else
                result.Errors.Should().Contain(e => e.PropertyName == propertyName);
        }

        // ---------- Tests ----------
        [Fact(DisplayName = "Should accept a fully valid request")]
        public void Should_Accept_Valid_Request()
        {
            var result = Validate();
            result.IsValid.Should().BeTrue(result.ToString());
        }

        [Theory(DisplayName = "Should trim and validate first/last names (min length 2)")]
        [InlineData("", "Doe")]
        [InlineData(" ", "Doe")]
        [InlineData("J", "Doe")]
        [InlineData("John", "")]
        [InlineData("John", " ")]
        [InlineData("John", "D")]
        public void Should_Reject_Invalid_Names(string first, string last)
        {
            var result = Validate(r => { r.FirstName = first; r.LastName = last; });

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.PropertyName == nameof(CreateEmployeeRequest.FirstName) ||
                e.PropertyName == nameof(CreateEmployeeRequest.LastName));
        }

        [Theory(DisplayName = "Should reject invalid email formats")]
        [InlineData("invalid")]
        [InlineData("john@")]
        [InlineData("john@acme")]
        [InlineData("john@@acme.com")]
        [InlineData("john..doe@acme.com")]
        public void Should_Reject_Invalid_Email(string email)
        {
            var result = Validate(r => r.Email = email);

            result.IsValid.Should().BeFalse();
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.Email));
        }

        [Theory(DisplayName = "Should accept valid CPF (masked or digits)")]
        [InlineData("52998224725")]
        [InlineData("529.982.247-25")]
        [InlineData("111.444.777-35")]
        public void Should_Accept_Valid_CPF(string cpf)
        {
            var result = Validate(r => r.DocumentNumber = cpf);

            result.IsValid.Should().BeTrue(result.ToString());
        }

        [Theory(DisplayName = "Should reject invalid CPF (length, repeated digits, wrong check digits)")]
        [InlineData("123")]              // curto
        [InlineData("123456789012")]     // longo
        [InlineData("00000000000")]      // repetidos
        [InlineData("11111111111")]      // repetidos
        [InlineData("12345678900")]      // DV inválido
        [InlineData("111.444.777-34")]   // máscara ok, DV inválido
        public void Should_Reject_Invalid_CPF(string cpf)
        {
            var result = Validate(r => r.DocumentNumber = cpf);

            result.IsValid.Should().BeFalse();
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.DocumentNumber));
        }

        [Fact(DisplayName = "Should require at least one phone number")]
        public void Should_Require_AtLeast_One_Phone()
        {
            var result = Validate(r => r.PhoneNumbers = new List<string>());

            result.IsValid.Should().BeFalse();
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.PhoneNumbers));
        }

        [Theory(DisplayName = "Should reject invalid phone numbers (BR)")]
        [InlineData("abc")]
        [InlineData("+55 +55 11 99999-9999")]
        [InlineData("11 89999-9999")] // mobile deve começar com 9
        public void Should_Reject_Invalid_Phones(string phone)
        {
            var result = Validate(r => r.PhoneNumbers = new List<string> { phone });

            result.IsValid.Should().BeFalse();
            // RuleForEach gera "PhoneNumbers[0]" etc.
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.PhoneNumbers), startsWith: true);
        }

        [Fact(DisplayName = "Should reject future birth dates")]
        public void Should_Reject_Future_BirthDate()
        {
            var result = Validate(r => r.DateOfBirth = DateTime.Today.AddDays(1).ToString(DateFormat));

            result.IsValid.Should().BeFalse();
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.DateOfBirth));
        }

        [Theory(DisplayName = "Should reject unparseable birth dates")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("31/02/2020")] // formato inválido para parser esperado
        [InlineData("not-a-date")]
        public void Should_Reject_Invalid_BirthDate_Format(string dob)
        {
            var result = Validate(r => r.DateOfBirth = dob);

            result.IsValid.Should().BeFalse();
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.DateOfBirth));
        }

        [Theory(DisplayName = "Should reject employees under 18 years old")]
        [InlineData(17)]  // 17 anos
        [InlineData(10)]  // 10 anos
        [InlineData(0)]   // bebê
        public void Should_Reject_Underage_Employees(int ageInYears)
        {
            var birthDate = DateTime.Today.AddYears(-ageInYears);
            var result = Validate(r => r.DateOfBirth = birthDate.ToString(DateFormat));

            result.IsValid.Should().BeFalse();
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.DateOfBirth));
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("at least 18 years old"));
        }

        [Theory(DisplayName = "Should accept employees 18 years or older")]
        [InlineData(18)]  // exatamente 18 anos
        [InlineData(25)]  // 25 anos
        [InlineData(65)]  // 65 anos
        public void Should_Accept_Adult_Employees(int ageInYears)
        {
            var birthDate = DateTime.Today.AddYears(-ageInYears);
            var result = Validate(r => r.DateOfBirth = birthDate.ToString(DateFormat));

            result.IsValid.Should().BeTrue(result.ToString());
        }

        [Theory(DisplayName = "Should validate password strength")]
        [InlineData("short1A")]     
        [InlineData("alllowercase1")]
        [InlineData("ALLUPPERCASE1")]
        [InlineData("NoDigitsAA")]
        public void Should_Reject_Weak_Passwords(string password)
        {
            var result = Validate(r => r.Password = password);

            result.IsValid.Should().BeFalse();
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.Password));
        }

        [Fact(DisplayName = "Should require non-empty DepartmentId")]
        public void Should_Require_DepartmentId()
        {
            var result = Validate(r => r.DepartmentId = Guid.Empty);

            result.IsValid.Should().BeFalse();
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.DepartmentId));
        }

        [Theory(DisplayName = "Should accept valid ManagerId (null or non-empty GUID)")]
        [InlineData(null)]
        [InlineData("12345678-1234-1234-1234-123456789012")]
        public void Should_Accept_Valid_ManagerId(string? managerId)
        {
            var result = Validate(r => 
            {
                if (managerId != null)
                    r.ManagerId = Guid.Parse(managerId);
                else
                    r.ManagerId = null;
            });

            result.IsValid.Should().BeTrue(result.ToString());
        }

        [Fact(DisplayName = "Should reject empty ManagerId")]
        public void Should_Reject_Empty_ManagerId()
        {
            var result = Validate(r => r.ManagerId = Guid.Empty);

            result.IsValid.Should().BeFalse();
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.ManagerId));
        }

        [Theory(DisplayName = "Should accept valid role levels")]
        [InlineData("Junior")]
        [InlineData("Pleno")]
        [InlineData("Senior")]
        [InlineData("Manager")]
        [InlineData("Director")]
        public void Should_Accept_Valid_Role_Levels(string roleLevel)
        {
            var result = Validate(r => r.RoleLevel = roleLevel);

            result.IsValid.Should().BeTrue(result.ToString());
        }

        [Theory(DisplayName = "Should reject invalid role levels")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Intern")]
        [InlineData("Lead")]
        [InlineData("VP")]
        public void Should_Reject_Invalid_Role_Levels(string roleLevel)
        {
            var result = Validate(r => r.RoleLevel = roleLevel);

            result.IsValid.Should().BeFalse();
            ShouldHaveErrorFor(result, nameof(CreateEmployeeRequest.RoleLevel));
        }

        [Theory(DisplayName = "Should be ok with trimmed fields (no false negatives)")]
        [InlineData("  John  ", "  Doe  ", "  john.doe@company.com ")]
        public void Should_Trim_Fields(string first, string last, string email)
        {
            var result = Validate(r =>
            {
                r.FirstName = first;
                r.LastName = last;
                r.Email = email;
            });

            result.IsValid.Should().BeTrue(result.ToString());
        }
    }
}
