using System.Threading;
using System.Threading.Tasks;
using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using System;
using System.Linq;

namespace CompanyManager.Application.Handlers
{
    public sealed class DeleteDepartmentCommandHandler : IDeleteDepartmentCommandHandler
    {
        private readonly IDepartmentRepository _departments;

        public DeleteDepartmentCommandHandler(IDepartmentRepository departments)
        {
            _departments = departments;
        }

        public async Task Handle(DeleteDepartmentCommand cmd, CancellationToken ct)
        {
            var existing = await _departments.GetByIdAsync(cmd.Id, ct);
            if (existing is null) return; // idempotente

            await _departments.DeleteAsync(cmd.Id, ct);
        }
    }
}
