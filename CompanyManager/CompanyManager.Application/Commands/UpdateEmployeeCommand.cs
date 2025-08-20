using System;

namespace CompanyManager.Application.Commands
{
    public class UpdateEmployeeCommand
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string[] Phones { get; set; } = Array.Empty<string>();
        public string JobTitle { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public Guid? ManagerId { get; set; } // Optional manager ID
    }
}
