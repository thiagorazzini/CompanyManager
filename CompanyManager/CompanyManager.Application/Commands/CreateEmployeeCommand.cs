using System;

namespace CompanyManager.Application.Commands
{
    public sealed class CreateEmployeeCommand
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string[] Phones { get; set; } = Array.Empty<string>();
        public string JobTitle { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public Guid? ManagerId { get; set; }
        public string Password { get; set; } = string.Empty;
        public string RoleLevel { get; set; } = string.Empty;
    }
}
