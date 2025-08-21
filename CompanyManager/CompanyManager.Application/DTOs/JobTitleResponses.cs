namespace CompanyManager.Application.DTOs
{
    public class JobTitleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int HierarchyLevel { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class CreateJobTitleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int HierarchyLevel { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }


}
