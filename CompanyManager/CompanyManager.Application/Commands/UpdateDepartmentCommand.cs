using System;

namespace CompanyManager.Application.Commands
{
    public sealed class UpdateDepartmentCommand
    {
        public Guid Id { get; init; }
        public string NewName { get; init; } = string.Empty;
    }
}
