namespace CompanyManager.Application.DTOs
{
    public sealed class CreateDepartmentRequest
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }
}
