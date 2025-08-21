namespace CompanyManager.Application.DTOs;

public sealed class GetEmployeeByIdResponse
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

public class JobTitleInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int HierarchyLevel { get; set; }
    public string Description { get; set; } = string.Empty;
}
