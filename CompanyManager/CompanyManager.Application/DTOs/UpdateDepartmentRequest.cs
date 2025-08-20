using System;

namespace CompanyManager.Application.DTOs
{
    public sealed class UpdateDepartmentRequest
    {
        public Guid Id { get; set; }
        public string NewName { get; set; } = string.Empty;
    }
}
