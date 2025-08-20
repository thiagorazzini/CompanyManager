using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.Entities;
using System;

namespace CompanyManager.Application.Queries
{
    public sealed class GetDepartmentByIdQueryHandler : IGetDepartmentByIdQueryHandler
    {
        private readonly IDepartmentRepository _departments;

        public GetDepartmentByIdQueryHandler(IDepartmentRepository departments)
        {
            _departments = departments;
        }

        public Task<Department?> Handle(Guid id, CancellationToken ct) =>
            _departments.GetByIdAsync(id, ct);
    }
}
