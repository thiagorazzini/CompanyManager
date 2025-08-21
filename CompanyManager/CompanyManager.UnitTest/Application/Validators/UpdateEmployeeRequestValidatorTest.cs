using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using FluentAssertions;

namespace CompanyManager.UnitTest.Application.Validators
{
    public sealed class UpdateEmployeeRequestValidatorTest
    {
        private readonly UpdateEmployeeRequestValidator _validator = new();

        private static UpdateEmployeeRequest Valid()
        {
            return new UpdateEmployeeRequest
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@company.com",
                DocumentNumber = "52998224725", // CPF válido
                PhoneNumbers = new List<string> { "11 99999-9999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid()
            };
        }

        private FluentValidation.Results.ValidationResult Validate(Action<UpdateEmployeeRequest>? mutate = null)
        {
            var req = Valid();
            mutate?.Invoke(req);
            return _validator.Validate(req);
        }

        [Fact(DisplayName = "Should accept a fully valid request")]
        public void Should_Accept_Valid_Request()
        {
            var req = Valid();

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Fact(DisplayName = "Should require non-empty Id")]
        public void Should_Require_Id()
        {
            var req = Valid();
            req.Id = Guid.Empty;

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeRequest.Id));
        }

        [Theory(DisplayName = "Should trim and validate first/last names (min length 2)")]
        [InlineData("", "Doe")]
        [InlineData(" ", "Doe")]
        [InlineData("J", "Doe")]
        [InlineData("Jane", "")]
        [InlineData("Jane", " ")]
        [InlineData("Jane", "D")]
        public void Should_Reject_Invalid_Names(string first, string last)
        {
            var req = Valid();
            req.FirstName = first;
            req.LastName = last;

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Select(e => e.PropertyName)
                .Should().Contain(p => p == nameof(UpdateEmployeeRequest.FirstName) || p == nameof(UpdateEmployeeRequest.LastName));
        }

        [Theory(DisplayName = "Should reject invalid email formats")]
        [InlineData("invalid")]
        [InlineData("john@")]
        [InlineData("john@acme")]
        [InlineData("john@@acme.com")]
        [InlineData("john..doe@acme.com")]
        public void Should_Reject_Invalid_Email(string email)
        {
            var req = Valid();
            req.Email = email;

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeRequest.Email));
        }

        [Theory(DisplayName = "Should accept valid CPF (masked or digits)")]
        [InlineData("52998224725")]
        [InlineData("529.982.247-25")]
        [InlineData("111.444.777-35")]
        public void Should_Accept_Valid_CPF(string cpf)
        {
            var req = Valid();
            req.DocumentNumber = cpf;

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Theory(DisplayName = "Should reject invalid CPF (length, repeated digits, wrong check digits)")]
        [InlineData("123")]
        [InlineData("123456789012")]
        [InlineData("00000000000")]
        [InlineData("11111111111")]
        [InlineData("12345678900")]
        [InlineData("111.444.777-34")]
        public void Should_Reject_Invalid_CPF(string cpf)
        {
            var req = Valid();
            req.DocumentNumber = cpf;

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeRequest.DocumentNumber));
        }

        [Fact(DisplayName = "Should require at least one phone number")]
        public void Should_Require_At_Least_One_Phone()
        {
            var req = Valid();
            req.PhoneNumbers = new List<string>();

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeRequest.PhoneNumbers));
        }

        [Theory(DisplayName = "Should reject invalid phone numbers (BR)")]
        [InlineData("abc")]
        [InlineData("+55 +55 11 99999-9999")]
        [InlineData("11 89999-9999")] // mobile deve começar com 9
        public void Should_Reject_Invalid_Phones(string phone)
        {
            var req = Valid();
            req.PhoneNumbers = new List<string> { phone };

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeRequest.PhoneNumbers));
        }

        [Theory(DisplayName = "Should accept valid phone numbers (BR)")]
        [InlineData("11 99999-9999")] // celular válido
        [InlineData("(11) 99999-9999")] // celular com parênteses
        [InlineData("11 8888-8888")] // fixo válido (10 dígitos)
        [InlineData("11 7777-7777")] // fixo válido (10 dígitos)
        [InlineData("+55 11 99999-9999")] // com código do país
        public void Should_Accept_Valid_Phones(string phone)
        {
            var req = Valid();
            req.PhoneNumbers = new List<string> { phone };

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Fact(DisplayName = "Should require non-empty DepartmentId")]
        public void Should_Require_DepartmentId()
        {
            var req = Valid();
            req.DepartmentId = Guid.Empty;

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeRequest.DepartmentId));
        }

        [Theory(DisplayName = "Should trim fields (no false negatives)")]
        [InlineData("  Jane  ", "  Doe  ", "  jane.doe@company.com ")]
        public void Should_Trim_Fields(string first, string last, string email)
        {
            var req = Valid();
            req.FirstName = first;
            req.LastName = last;
            req.Email = email;

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeTrue(r.ToString());
        }

        [Fact(DisplayName = "Should require non-empty JobTitleId")]
        public void Should_Require_JobTitleId()
        {
            var req = Valid();
            req.JobTitleId = Guid.Empty;

            var v = new UpdateEmployeeRequestValidator();
            var r = v.Validate(req);

            r.IsValid.Should().BeFalse();
            r.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmployeeRequest.JobTitleId));
        }
    }
}
