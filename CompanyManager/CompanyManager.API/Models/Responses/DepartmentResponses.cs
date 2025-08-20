using CompanyManager.API.Models;

namespace CompanyManager.API.Models.Responses
{
    public class DepartmentListResponse : BaseResponse
    {
        public List<DepartmentSummaryDto> Items { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
    }

    public class DepartmentSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DepartmentDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DepartmentCreatedResponse : BaseResponse
    {
        public Guid DepartmentId { get; set; }
    }

    public class DepartmentUpdatedResponse : BaseResponse
    {
    }

    public class DepartmentDeletedResponse : BaseResponse
    {
    }
}
