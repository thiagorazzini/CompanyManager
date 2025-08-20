using System;
using System.Collections.Generic;

namespace CompanyManager.Application.DTOs
{
    public class CreateEmployeeRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public IEnumerable<string> PhoneNumbers { get; set; } = Array.Empty<string>();
        public string JobTitle { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public Guid? ManagerId { get; set; } // Optional manager ID
        public string RoleLevel { get; set; } = "Junior"; // Default role level
        public string Password { get; set; } = string.Empty;
    }
}
