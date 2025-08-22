using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.Common;
using CompanyManager.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyManager.UnitTest.Application.TestDouble
{
    internal sealed class StubDepartmentRepository : IDepartmentRepository
    {
        private readonly HashSet<Guid> _existing;

        public StubDepartmentRepository(IEnumerable<Guid> existingDepartments)
        {
            _existing = new(existingDepartments);
        }

        public Task<Department?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<Department?>(null);

        public Task<bool> ExistsAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(_existing.Contains(id));

        public Task AddAsync(Department department, CancellationToken ct) =>
            Task.CompletedTask;

        public Task UpdateAsync(Department department, CancellationToken ct) =>
            Task.CompletedTask;

        public Task DeleteAsync(Guid id, CancellationToken ct) =>
            Task.CompletedTask;

        public Task<(IReadOnlyList<Department> Items, int Total)> SearchAsync(
            DepartmentFilter filter, PageRequest page, CancellationToken ct)
            => Task.FromResult(((IReadOnlyList<Department>)Array.Empty<Department>(), 0));

        // Métodos faltantes da interface
        public Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Department?>(null);
        }

        public Task<Department?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Department?>(null);
        }

        public Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<Department>());
        }

        public Task<IEnumerable<Department>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<Department>());
        }

        public Task<IEnumerable<Department>> GetInactiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<Department>());
        }

        public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<bool> ExistsByDescriptionAsync(string description, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<IEnumerable<Department>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<Department>());
        }

        public Task<int> GetEmployeeCountAsync(Guid departmentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task<IEnumerable<Department>> GetDepartmentsWithEmployeeCountAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<Department>());
        }
    }
}
