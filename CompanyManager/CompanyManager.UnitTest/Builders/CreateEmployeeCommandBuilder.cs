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
            Phones = new List<string> { "11999999999" },
            JobTitleId = Guid.NewGuid(),
            DateOfBirth = DateTime.Today.AddYears(-25).ToString("yyyy-MM-dd"),
            Password = "Strong123!",
            DepartmentId = Guid.NewGuid()
        };

        public CreateEmployeeCommandBuilder()
        {
            _command = new CreateEmployeeCommand
            {
                FirstName = "João",
                LastName = "Silva",
                Email = "joao.silva@empresa.com",
                DocumentNumber = "52998224725",
                DateOfBirth = "1990-01-01",
                Phones = new List<string> { "+5511999999999" },
                JobTitleId = Guid.NewGuid(),
                DepartmentId = Guid.NewGuid(),
                Password = "Senha123!"
            };
        }

        public CreateEmployeeCommandBuilder WithFirstName(string firstName) { _command.FirstName = firstName; return this; }
        public CreateEmployeeCommandBuilder WithLastName(string lastName) { _command.LastName = lastName; return this; }
        public CreateEmployeeCommandBuilder WithEmail(string email) { _command.Email = email; return this; }
        public CreateEmployeeCommandBuilder WithDocumentNumber(string documentNumber) { _command.DocumentNumber = documentNumber; return this; }
        public CreateEmployeeCommandBuilder WithDateOfBirth(string dateOfBirth) { _command.DateOfBirth = dateOfBirth; return this; }
        public CreateEmployeeCommandBuilder WithPhoneNumbers(IEnumerable<string> phoneNumbers) { _command.Phones = phoneNumbers.ToList(); return this; }
        public CreateEmployeeCommandBuilder WithJobTitleId(Guid jobTitleId) { _command.JobTitleId = jobTitleId; return this; }
        public CreateEmployeeCommandBuilder WithDepartment(Guid departmentId) { _command.DepartmentId = departmentId; return this; }
        public CreateEmployeeCommandBuilder WithPassword(string password) { _command.Password = password; return this; }

        public CreateEmployeeCommand Build() => _command;

        public static CreateEmployeeCommandBuilder New() => new CreateEmployeeCommandBuilder();
    }
}
