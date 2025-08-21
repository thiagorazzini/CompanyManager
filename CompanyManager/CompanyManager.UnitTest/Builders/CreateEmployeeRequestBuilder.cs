using CompanyManager.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanyManager.UnitTest.Builders
{
    public sealed class CreateEmployeeRequestBuilder
    {
        private readonly CreateEmployeeRequest _req = new()
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@company.com",
            DocumentNumber = "52998224725",
            PhoneNumbers = new List<string> { "11999999999" },
            DateOfBirth = DateTime.Today.AddYears(-25).ToString("yyyy-MM-dd"),
            Password = "Strong123!",
            DepartmentId = Guid.NewGuid()
        };

        public CreateEmployeeRequestBuilder()
        {
            _req = new CreateEmployeeRequest
            {
                FirstName = "João",
                LastName = "Silva",
                Email = "joao.silva@empresa.com",
                DocumentNumber = "12345678901",
                DateOfBirth = "1990-01-01",
                PhoneNumbers = new List<string> { "+5511999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "Senha123!"
            };
        }

        public CreateEmployeeRequestBuilder WithFirstName(string firstName) { _req.FirstName = firstName; return this; }
        public CreateEmployeeRequestBuilder WithLastName(string lastName) { _req.LastName = lastName; return this; }
        public CreateEmployeeRequestBuilder WithEmail(string email) { _req.Email = email; return this; }
        public CreateEmployeeRequestBuilder WithDocumentNumber(string documentNumber) { _req.DocumentNumber = documentNumber; return this; }
        public CreateEmployeeRequestBuilder WithDateOfBirth(string dateOfBirth) { _req.DateOfBirth = dateOfBirth; return this; }
        public CreateEmployeeRequestBuilder WithPhoneNumbers(IEnumerable<string> phoneNumbers) { _req.PhoneNumbers = phoneNumbers.ToList(); return this; }
        public CreateEmployeeRequestBuilder WithJobTitleId(Guid jobTitleId) { _req.JobTitleId = jobTitleId; return this; }
        public CreateEmployeeRequestBuilder WithDepartment(Guid departmentId) { _req.DepartmentId = departmentId; return this; }
        public CreateEmployeeRequestBuilder WithPassword(string password) { _req.Password = password; return this; }

        public CreateEmployeeRequest Build() => _req;

        public static CreateEmployeeRequestBuilder New() => new CreateEmployeeRequestBuilder();
    }
}
