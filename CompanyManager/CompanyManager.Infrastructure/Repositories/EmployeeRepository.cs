using CompanyManager.Domain.Common;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompanyManager.Infrastructure.Repositories
{
    /// <summary>
    /// SQL Server implementation of the employee repository using Entity Framework Core.
    /// </summary>
    public sealed class EmployeeRepository : IEmployeeRepository
    {
        private readonly CompanyContext _context;

        public EmployeeRepository(CompanyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Métodos de leitura
        public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Email.Value == email, cancellationToken);
        }

        public async Task<Employee?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(documentNumber))
                return null;

            return await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.DocumentNumber.Digits == documentNumber || e.DocumentNumber.Raw == documentNumber, cancellationToken);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Employees
                .AsNoTracking()
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
        {
            return await _context.Employees
                .AsNoTracking()
                .Where(e => e.DepartmentId == departmentId)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Employee>> GetByJobTitleAsync(string jobTitle, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jobTitle))
                return Enumerable.Empty<Employee>();

            return await _context.Employees
                .AsNoTracking()
                .Where(e => e.JobTitle.Contains(jobTitle))
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default)
        {
            // A entidade Employee não tem campo IsActive
            // Retornar todos os funcionários
            return await _context.Employees
                .AsNoTracking()
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync(cancellationToken);
        }

        public Task<IEnumerable<Employee>> GetInactiveEmployeesAsync(CancellationToken cancellationToken = default)
        {
            // A entidade Employee não tem campo IsActive
            // Retornar lista vazia
            return Task.FromResult(Enumerable.Empty<Employee>());
        }

        // Métodos de busca com filtros e paginação
        public async Task<(IReadOnlyList<Employee> Items, int Total)> SearchAsync(
            EmployeeFilter filter, PageRequest page, CancellationToken cancellationToken = default)
        {
            var query = _context.Employees.AsNoTracking();

            // Aplicar filtros
            if (filter.DepartmentId.HasValue)
                query = query.Where(e => e.DepartmentId == filter.DepartmentId.Value);

            if (!string.IsNullOrWhiteSpace(filter.NameOrEmail))
            {
                var searchTerm = filter.NameOrEmail.ToLowerInvariant();
                query = query.Where(e =>
                    e.FirstName.ToLower().Contains(searchTerm) ||
                    e.LastName.ToLower().Contains(searchTerm) ||
                    e.Email.Value.ToLower().Contains(searchTerm));
            }

            // Contar total
            var total = await query.CountAsync(cancellationToken);

            // Aplicar paginação e ordenação
            var items = await query
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Skip((page.Page - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        // Métodos de verificação de existência
        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Employees
                .AsNoTracking()
                .AnyAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _context.Employees
                .AsNoTracking()
                .AnyAsync(e => e.Email.Value == email, cancellationToken);
        }

        public async Task<bool> ExistsByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(documentNumber))
                return false;

            return await _context.Employees
                .AsNoTracking()
                .AnyAsync(e => e.DocumentNumber.Digits == documentNumber || e.DocumentNumber.Raw == documentNumber, cancellationToken);
        }

        public async Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(normalizedEmail))
                return false;

            return await _context.Employees
                .AsNoTracking()
                .AnyAsync(e => e.Email.Value.ToLower() == normalizedEmail.ToLower(), cancellationToken);
        }

        public async Task<bool> CpfExistsAsync(string cpfDigitsOnly, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cpfDigitsOnly))
                return false;

            return await _context.Employees
                .AsNoTracking()
                .AnyAsync(e => e.DocumentNumber.Digits == cpfDigitsOnly, cancellationToken);
        }

        // Métodos de CRUD
        public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            if (employee is null)
                throw new ArgumentNullException(nameof(employee));

            await _context.Employees.AddAsync(employee, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            if (employee is null)
                throw new ArgumentNullException(nameof(employee));

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var employee = await GetByIdAsync(id, cancellationToken);
            if (employee != null)
            {
                // A entidade Employee não tem campo IsActive
                // Remover fisicamente do banco
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        // Métodos de consulta em lote
        public async Task<IEnumerable<Employee>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any())
                return Enumerable.Empty<Employee>();

            return await _context.Employees
                .AsNoTracking()
                .Where(e => ids.Contains(e.Id))
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetCountByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
        {
            return await _context.Employees
                .AsNoTracking()
                .CountAsync(e => e.DepartmentId == departmentId, cancellationToken);
        }

        public async Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
        {
            // A entidade Employee não tem campo IsActive
            // Retornar total de funcionários
            return await _context.Employees
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }
    }
}
