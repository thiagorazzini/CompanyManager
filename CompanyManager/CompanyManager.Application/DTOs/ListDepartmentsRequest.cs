using System;

namespace CompanyManager.Application.DTOs
{
    public sealed class ListDepartmentsRequest
    {
        public string? NameContains { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
