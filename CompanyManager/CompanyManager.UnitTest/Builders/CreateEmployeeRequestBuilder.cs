using CompanyManager.Application.DTOs;
using System;
using System.Collections.Generic;

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
            JobTitle = "Developer",
            DateOfBirth = DateTime.Today.AddYears(-25).ToString("yyyy-MM-dd"),
            RoleLevel = "Junior",
            Password = "Strong123!",
            DepartmentId = Guid.NewGuid(),
            ManagerId = null
        };

        public CreateEmployeeRequestBuilder WithFirstName(string firstName) { _req.FirstName = firstName; return this; }
        public CreateEmployeeRequestBuilder WithLastName(string lastName) { _req.LastName = lastName; return this; }
        public CreateEmployeeRequestBuilder WithEmail(string email) { _req.Email = email; return this; }
        public CreateEmployeeRequestBuilder WithDocumentNumber(string documentNumber) { _req.DocumentNumber = documentNumber; return this; }
        public CreateEmployeeRequestBuilder WithDateOfBirth(string dateOfBirth) { _req.DateOfBirth = dateOfBirth; return this; }
        public CreateEmployeeRequestBuilder WithPhoneNumbers(IEnumerable<string> phoneNumbers) { _req.PhoneNumbers = phoneNumbers; return this; }
        public CreateEmployeeRequestBuilder WithJobTitle(string jobTitle) { _req.JobTitle = jobTitle; return this; }
        public CreateEmployeeRequestBuilder WithDepartment(Guid departmentId) { _req.DepartmentId = departmentId; return this; }
        public CreateEmployeeRequestBuilder WithManager(Guid? managerId) { _req.ManagerId = managerId; return this; }
        public CreateEmployeeRequestBuilder WithRoleLevel(string roleLevel) { _req.RoleLevel = roleLevel; return this; }
        public CreateEmployeeRequestBuilder WithPassword(string password) { _req.Password = password; return this; }

        public CreateEmployeeRequest Build() => _req;

        public static CreateEmployeeRequestBuilder New() => new CreateEmployeeRequestBuilder();
    }
}
