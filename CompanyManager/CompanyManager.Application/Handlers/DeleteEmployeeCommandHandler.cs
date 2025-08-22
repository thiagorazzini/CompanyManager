using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.Abstractions;

namespace CompanyManager.Application.Handlers
{
    public sealed class DeleteEmployeeCommandHandler : IDeleteEmployeeCommandHandler
    {
        private readonly IEmployeeRepository _employees;

        public DeleteEmployeeCommandHandler(IEmployeeRepository employees)
        {
            _employees = employees ?? throw new ArgumentNullException(nameof(employees));
        }

        public async Task Handle(DeleteEmployeeCommand cmd, CancellationToken ct)
        {
            var existing = await _employees.GetByIdAsync(cmd.Id, ct);
            if (existing is null) return; 

            await _employees.DeleteAsync(cmd.Id, ct);
        }
    }
}
