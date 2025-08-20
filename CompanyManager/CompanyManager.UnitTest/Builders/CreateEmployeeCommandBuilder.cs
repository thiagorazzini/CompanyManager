using CompanyManager.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanyManager.UnitTest.Builders
{
    public sealed class CreateEmployeeCommandBuilder
    {
        private readonly CreateEmployeeCommand _command = new()
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@company.com",
            DocumentNumber = "52998224725",
            Phones = new string[] { "11999999999" },
            JobTitle = "Developer",
            DateOfBirth = DateTime.Today.AddYears(-25).ToString("yyyy-MM-dd"),
            RoleLevel = "Junior",
            Password = "Strong123!",
            DepartmentId = Guid.NewGuid(),
            ManagerId = null
        };

        public CreateEmployeeCommandBuilder WithFirstName(string firstName) { _command.FirstName = firstName; return this; }
        public CreateEmployeeCommandBuilder WithLastName(string lastName) { _command.LastName = lastName; return this; }
        public CreateEmployeeCommandBuilder WithEmail(string email) { _command.Email = email; return this; }
        public CreateEmployeeCommandBuilder WithDocumentNumber(string documentNumber) { _command.DocumentNumber = documentNumber; return this; }
        public CreateEmployeeCommandBuilder WithDateOfBirth(string dateOfBirth) { _command.DateOfBirth = dateOfBirth; return this; }
        public CreateEmployeeCommandBuilder WithPhones(IEnumerable<string> phones) { _command.Phones = phones.ToArray(); return this; }
        public CreateEmployeeCommandBuilder WithJobTitle(string jobTitle) { _command.JobTitle = jobTitle; return this; }
        public CreateEmployeeCommandBuilder WithDepartment(Guid departmentId) { _command.DepartmentId = departmentId; return this; }
        public CreateEmployeeCommandBuilder WithManager(Guid? managerId) { _command.ManagerId = managerId; return this; }
        public CreateEmployeeCommandBuilder WithRoleLevel(string roleLevel) { _command.RoleLevel = roleLevel; return this; }
        public CreateEmployeeCommandBuilder WithPassword(string password) { _command.Password = password; return this; }

        public CreateEmployeeCommand Build() => _command;

        public static CreateEmployeeCommandBuilder New() => new CreateEmployeeCommandBuilder();
    }
}
