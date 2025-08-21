using CompanyManager.Domain.Common;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;

namespace CompanyManager.UnitTest.Application.TestDouble
{

    internal sealed class ProbingEmployeeRepository : IEmployeeRepository
    {
        private readonly Dictionary<Guid, Employee> _store = new();

        public HashSet<string> TakenEmails { get; init; } =
            new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> TakenCpfs { get; init; } = new();

        public int EmailExistsCalls { get; private set; }
        public int CpfExistsCalls { get; private set; }

        public ProbingEmployeeRepository SeedEmail(string normalizedEmail)
        {
            TakenEmails.Add(normalizedEmail);
            return this;
        }

        public ProbingEmployeeRepository SeedCpf(string cpfDigitsOnly)
        {
            TakenCpfs.Add(cpfDigitsOnly);
            return this;
        }

        public ProbingEmployeeRepository(Employee? seed)
        {
            if (seed is not null) _store[seed.Id] = seed;
        }

        public Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);

        public Task<Employee?> GetByIdWithJobTitleAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);

        public Task AddAsync(Employee employee, CancellationToken ct)
        {
            _store[employee.Id] = employee;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Employee employee, CancellationToken ct)
        {
            _store[employee.Id] = employee;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            _store.Remove(id);
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<Employee> Items, int Total)> SearchAsync(
            EmployeeFilter filter, PageRequest page, CancellationToken ct)
        {
            var items = _store.Values.ToList();
            return Task.FromResult(((IReadOnlyList<Employee>)items, items.Count));
        }

        public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct)
        {
            EmailExistsCalls++;
            return Task.FromResult(TakenEmails.Contains(normalizedEmail));
        }

        public Task<bool> CpfExistsAsync(string cpfDigitsOnly, CancellationToken ct)
        {
            CpfExistsCalls++;
            return Task.FromResult(TakenCpfs.Contains(cpfDigitsOnly));
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
            return Task.FromResult(_store.Values.Where(e => e.JobTitle?.Name.Contains(jobTitle ?? "") == true));
        }

        public Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_store.Values.AsEnumerable());
        }

        public Task<IEnumerable<Employee>> GetInactiveEmployeesAsync(CancellationToken cancellationToken = default)
        {
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
            return Task.FromResult(_store.Count);
        }

        public Task<IEnumerable<string>> GetDistinctJobTitlesAsync(CancellationToken cancellationToken = default)
        {
            var distinctJobTitles = _store.Values
                .Where(e => e.JobTitle != null)
                .Select(e => e.JobTitle.Name)
                .Distinct()
                .OrderBy(title => title)
                .ToList();
            
            return Task.FromResult(distinctJobTitles.AsEnumerable());
        }
    }
}
