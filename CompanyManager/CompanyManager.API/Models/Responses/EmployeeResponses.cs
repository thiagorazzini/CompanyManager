using CompanyManager.API.Models;

namespace CompanyManager.API.Models.Responses
{
    public class EmployeeListResponse : BaseResponse
    {
        public List<EmployeeSummaryDto> Items { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    public class EmployeeSummaryDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EmployeeDetailDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public List<string> PhoneNumbers { get; set; } = new();
        public Guid JobTitleId { get; set; }
        public string JobTitleName { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class EmployeeCreatedResponse : BaseResponse
    {
        public Guid EmployeeId { get; set; }
    }

    public class EmployeeUpdatedResponse : BaseResponse
    {
    }

    public class EmployeeDeletedResponse : BaseResponse
    {
    }
}
