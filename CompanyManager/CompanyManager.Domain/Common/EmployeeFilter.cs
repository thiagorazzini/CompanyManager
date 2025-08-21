namespace CompanyManager.Domain.Common
{
    public sealed record EmployeeFilter(
        Guid? DepartmentId = null, 
        string? NameOrEmail = null,
        Guid? JobTitleId = null);
}
