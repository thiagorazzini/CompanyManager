namespace CompanyManager.Application.Common
{
    public sealed record EmployeeFilter(
        string? NameOrEmail = null,
        Guid? DepartmentId = null,
        Guid? JobTitleId = null
    );
}
