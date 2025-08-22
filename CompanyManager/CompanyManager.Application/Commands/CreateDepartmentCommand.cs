namespace CompanyManager.Application.Commands
{
    public sealed class CreateDepartmentCommand
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }
}
