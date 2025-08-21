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
        public List<string> PhoneNumbers { get; set; } = new();
        public Guid JobTitleId { get; set; }
        public Guid DepartmentId { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
