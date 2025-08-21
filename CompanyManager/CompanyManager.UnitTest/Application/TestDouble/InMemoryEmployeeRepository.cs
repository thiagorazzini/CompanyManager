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
    internal sealed class InMemoryEmployeeRepository : IEmployeeRepository
    {
        private readonly Dictionary<Guid, Employee> _store = new();
        private readonly HashSet<string> _emails = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _cpfs = new();

        public InMemoryEmployeeRepository SeedEmail(string normalizedEmail)
        {
            _emails.Add(normalizedEmail);
            return this;
        }

        public InMemoryEmployeeRepository SeedCpf(string cpfDigitsOnly)
        {
            _cpfs.Add(cpfDigitsOnly);
            return this;
        }

        public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct) =>
            Task.FromResult(_emails.Contains(normalizedEmail));

        public Task<bool> CpfExistsAsync(string cpfDigitsOnly, CancellationToken ct) =>
            Task.FromResult(_cpfs.Contains(cpfDigitsOnly));

        public Task AddAsync(Employee employee, CancellationToken ct)
        {
            _store[employee.Id] = employee;
            _emails.Add(employee.Email.Value.ToLowerInvariant());
            _cpfs.Add(employee.DocumentNumber.Digits);
            return Task.CompletedTask;
        }

        public Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);

        public Task<Employee?> GetByIdWithJobTitleAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);


        public Task UpdateAsync(Employee employee, CancellationToken ct)
        {
            if (_store.TryGetValue(employee.Id, out var old))
            {
                _emails.Remove(old.Email.Value.ToLowerInvariant());
                _cpfs.Remove(old.DocumentNumber.Digits);
            }

            _store[employee.Id] = employee;
            _emails.Add(employee.Email.Value.ToLowerInvariant());
            _cpfs.Add(employee.DocumentNumber.Digits);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            if (_store.TryGetValue(id, out var existing))
            {
                _store.Remove(id);
                _emails.Remove(existing.Email.Value.ToLowerInvariant());
                _cpfs.Remove(existing.DocumentNumber.Digits);
            }
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<Employee> Items, int Total)> SearchAsync(
            EmployeeFilter filter, PageRequest page, CancellationToken ct)
        {
            IEnumerable<Employee> query = _store.Values;

            if (filter.NameOrEmail != null)
            {
                var term = filter.NameOrEmail.Trim().ToLowerInvariant();
                query = query.Where(e =>
                    ($"{e.FirstName} {e.LastName}".ToLowerInvariant().Contains(term)) ||
                    e.Email.Value.ToLowerInvariant().Contains(term));
            }

                    if (filter.JobTitleId.HasValue)
        {
            query = query.Where(e => e.JobTitleId == filter.JobTitleId.Value);
        }

            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty)
            {
                var deptId = filter.DepartmentId.Value;
                query = query.Where(e => e.DepartmentId == deptId);
            }

            var total = query.Count();
            var items = query
                .Skip(page.Offset)
                .Take(page.Take)
                .ToList();

            return Task.FromResult(((IReadOnlyList<Employee>)items, total));
        }

        // Métodos faltantes da interface
        public Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.FirstOrDefault(e => e.Email.Value == email));
        }

        public Task<Employee?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.FirstOrDefault(e => e.DocumentNumber.Digits == documentNumber || e.DocumentNumber.Raw == documentNumber));
        }

        public Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.AsEnumerable());
        }

        public Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Where(e => e.DepartmentId == departmentId));
        }

        public Task<IEnumerable<Employee>> GetByJobTitleAsync(string jobTitle, CancellationToken cancellationToken = default)
        {
            // Este método agora busca por nome do JobTitle, não por string direta
            return Task.FromResult(_store.Values.Where(e => e.JobTitle?.Name.Contains(jobTitle ?? "") == true));
        }

        public Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default)
        {
            // A entidade Employee não tem campo IsActive - retornar todos
            return Task.FromResult(_store.Values.AsEnumerable());
        }

        public Task<IEnumerable<Employee>> GetInactiveEmployeesAsync(CancellationToken cancellationToken = default)
        {
            // A entidade Employee não tem campo IsActive - retornar vazio
            return Task.FromResult(Enumerable.Empty<Employee>());
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.ContainsKey(id));
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Any(e => e.Email.Value == email));
        }

        public Task<bool> ExistsByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Any(e => e.DocumentNumber.Digits == documentNumber || e.DocumentNumber.Raw == documentNumber));
        }

        public Task<IEnumerable<Employee>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Where(e => ids.Contains(e.Id)));
        }

        public Task<int> GetCountByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.Count(e => e.DepartmentId == departmentId));
        }

        public Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
        {
            // A entidade Employee não tem campo IsActive - retornar total
            return Task.FromResult(_store.Count);
        }

        public Task<IEnumerable<string>> GetDistinctJobTitlesAsync(CancellationToken cancellationToken = default)
        {
            var distinctJobTitles = _store.Values
                .Where(e => e.JobTitle != null)
                .Select(e => e.JobTitle.Name)
                .Distinct()
                .OrderBy(jobTitle => jobTitle)
                .ToList();
            
            return Task.FromResult(distinctJobTitles.AsEnumerable());
        }
    }
}
