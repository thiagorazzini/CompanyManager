using System;
using System.Collections.Generic;

namespace CompanyManager.Application.DTOs
{
    public class UpdateEmployeeRequest
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public IEnumerable<string> PhoneNumbers { get; set; } = Array.Empty<string>();
        public Guid JobTitleId { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid? ManagerId { get; set; } // Optional manager ID
        public string? Password { get; set; } // Optional password for updates
    }
}
