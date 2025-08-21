using System;
using System.Collections.Generic;

namespace CompanyManager.Application.Commands
{
    public class UpdateEmployeeCommand
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public List<string> Phones { get; set; } = new();
        public Guid JobTitleId { get; set; }
        public Guid DepartmentId { get; set; }
        public string? Password { get; set; } // Optional password for updates
    }
}
