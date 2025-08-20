using System;

namespace CompanyManager.Application.DTOs
{
    public sealed class ListEmployeesRequest
    {
        public Guid? DepartmentId { get; set; }
        public string? NameContains { get; set; }
        public string? JobTitle { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
