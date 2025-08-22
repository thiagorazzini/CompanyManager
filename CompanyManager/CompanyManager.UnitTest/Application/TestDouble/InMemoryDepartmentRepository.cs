using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.Common;   // DepartmentFilter, PageRequest
using CompanyManager.Domain.Entities;

namespace CompanyManager.UnitTest.Application.TestDouble
{
    internal sealed class InMemoryDepartmentRepository : IDepartmentRepository
    {
        private readonly Dictionary<Guid, Department> _store = new();
        private readonly HashSet<Guid> _existing;

        public InMemoryDepartmentRepository()
        {
            _existing = new HashSet<Guid>();
        }

        // Mantido para cenários onde você só quer simular existência
        public InMemoryDepartmentRepository(IEnumerable<Guid> existingDepartments)
        {
            _existing = new HashSet<Guid>(existingDepartments);
        }

        public Task<bool> ExistsAsync(Guid departmentId, CancellationToken ct) =>
            Task.FromResult(_existing.Contains(departmentId) || _store.ContainsKey(departmentId));

        public Task<Department?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(_store.TryGetValue(id, out var d) ? d : null);

        public Task AddAsync(Department department, CancellationToken ct)
        {
            _store[department.Id] = department;
            _existing.Add(department.Id); // garante que ExistsAsync funcione também por este set
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Department department, CancellationToken ct)
        {
            _store[department.Id] = department;
            _existing.Add(department.Id);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            _store.Remove(id);
            _existing.Remove(id);
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<Department> Items, int Total)> SearchAsync(
            DepartmentFilter filter, PageRequest page, CancellationToken ct)
        {
            IEnumerable<Department> query = _store.Values;

            if (!string.IsNullOrWhiteSpace(filter.NameContains))
            {
                var term = filter.NameContains.Trim().ToLowerInvariant();
                query = query.Where(d => (d.Name ?? string.Empty).ToLowerInvariant().Contains(term));
            }

            var total = query.Count();
            var items = query
                .Skip(page.Offset)
                .Take(page.Take)
                .ToList();

            return Task.FromResult(((IReadOnlyList<Department>)items, total));
        }

        // Métodos faltantes da interface
        public Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.FirstOrDefault(d => d.Name == name));
        }

        public Task<Department?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.FirstOrDefault(d => d.Description == description));
        }

        public Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.AsEnumerable());
        }

        public Task<IEnumerable<Department>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Where(d => d.IsActive));
        }

        public Task<IEnumerable<Department>> GetInactiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Where(d => !d.IsActive));
        }

        public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Any(d => d.Name == name));
        }

        public Task<bool> ExistsByDescriptionAsync(string description, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Any(d => d.Description == description));
        }

        public Task<IEnumerable<Department>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Where(d => ids.Contains(d.Id)));
        }

        public Task<int> GetEmployeeCountAsync(Guid departmentId, CancellationToken cancellationToken = default)
        {
            // Simular contagem de funcionários - em testes reais isso seria calculado
            return Task.FromResult(0);
        }

        public Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Count(d => d.IsActive));
        }

        public Task<IEnumerable<Department>> GetDepartmentsWithEmployeeCountAsync(CancellationToken cancellationToken = default)
        {
            // Simular retorno com contagem de funcionários
            return Task.FromResult(_store.Values.AsEnumerable());
        }
    }
}
