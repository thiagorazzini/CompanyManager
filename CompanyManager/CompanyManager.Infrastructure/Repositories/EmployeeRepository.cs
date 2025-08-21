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
            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .Include(e => e.JobTitle)
                    .Include(e => e.Phones)  // ✅ Incluir telefones
                    .ToListAsync(cancellationToken);
                
                return allEmployees.FirstOrDefault(e => e.Id == id);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .Include(e => e.JobTitle)
                    .Include(e => e.Phones)  // ✅ Incluir telefones
                    .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            }
        }

        public async Task<Employee?> GetByIdWithJobTitleAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Employees
                .Include(e => e.JobTitle)
                .Include(e => e.Phones)  // ✅ Incluir telefones
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees.FirstOrDefault(e => e.Email.Value == email);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Email.Value == email, cancellationToken);
            }
        }

        public async Task<Employee?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(documentNumber))
                return null;

            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees.FirstOrDefault(e => e.DocumentNumber.Digits == documentNumber || e.DocumentNumber.Raw == documentNumber);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.DocumentNumber.Digits == documentNumber || e.DocumentNumber.Raw == documentNumber, cancellationToken);
            }
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees
                    .Where(e => e.DepartmentId == departmentId)
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .Where(e => e.DepartmentId == departmentId)
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<Employee>> GetByJobTitleAsync(string jobTitle, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jobTitle))
                return Enumerable.Empty<Employee>();

            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .Include(e => e.JobTitle)
                    .Include(e => e.Phones)  // ✅ Incluir telefones
                    .ToListAsync(cancellationToken);
                
                return allEmployees
                    .Where(e => e.JobTitle != null && e.JobTitle.Name.Contains(jobTitle))
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .Include(e => e.JobTitle)
                    .Include(e => e.Phones)  // ✅ Incluir telefones
                    .Where(e => e.JobTitle != null && e.JobTitle.Name.Contains(jobTitle))
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync(cancellationToken);
            }
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
            var query = _context.Employees
                .AsNoTracking()
                .Include(e => e.JobTitle)
                .Include(e => e.Department)
                .Include(e => e.Phones)  // ✅ Incluir telefones
                .AsQueryable();

            // Aplicar filtros
            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty)
                query = query.Where(e => e.DepartmentId == filter.DepartmentId.Value);

            if (!string.IsNullOrWhiteSpace(filter.NameOrEmail))
            {
                var searchTerm = filter.NameOrEmail.ToLowerInvariant();
                query = query.Where(e =>
                    EF.Functions.Like(e.FirstName.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(e.LastName.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(e.Email.Value, $"%{searchTerm}%"));
            }

            if (filter.JobTitleId.HasValue)
                query = query.Where(e => e.JobTitleId == filter.JobTitleId.Value);

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
            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees.Any(e => e.Id == id);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .AnyAsync(e => e.Id == id, cancellationToken);
            }
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees.Any(e => e.Email.Value == email);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .CountAsync(e => e.Email.Value == email, cancellationToken) > 0;
            }
        }

        public async Task<bool> ExistsByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(documentNumber))
                return false;

            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees.Any(e => e.DocumentNumber.Raw == documentNumber);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .CountAsync(e => e.DocumentNumber.Raw == documentNumber, cancellationToken) > 0;
            }
        }

        public async Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(normalizedEmail))
                return false;

            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees.Any(e => e.Email.Value == normalizedEmail);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .CountAsync(e => e.Email.Value == normalizedEmail, cancellationToken) > 0;
            }
        }

        public async Task<bool> CpfExistsAsync(string cpfDigitsOnly, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cpfDigitsOnly))
                return false;

            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees.Any(e => e.DocumentNumber.Raw == cpfDigitsOnly);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .CountAsync(e => e.DocumentNumber.Raw == cpfDigitsOnly, cancellationToken) > 0;
            }
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

            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees
                    .Where(e => ids.Contains(e.Id))
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .Where(e => ids.Contains(e.Id))
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync(cancellationToken);
            }
        }

        public async Task<int> GetCountByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees.Count(e => e.DepartmentId == departmentId);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .CountAsync(e => e.DepartmentId == departmentId, cancellationToken);
            }
        }

        public async Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
        {
            // A entidade Employee não tem campo IsActive
            // Retornar total de funcionários
            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
                
                return allEmployees.Count;
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .CountAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<string>> GetDistinctJobTitlesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Usar client-side evaluation para evitar problemas de LINQ translation
                var allEmployees = await _context.Employees
                    .AsNoTracking()
                    .Include(e => e.JobTitle)
                    .Include(e => e.Phones)  // ✅ Incluir telefones
                    .ToListAsync(cancellationToken);
                
                return allEmployees
                    .Where(e => e.JobTitle != null)
                    .Select(e => e.JobTitle.Name)
                    .Distinct()
                    .OrderBy(jobTitle => jobTitle);
            }
            catch (OperationCanceledException)
            {
                // Se a operação foi cancelada, tentar uma abordagem mais simples
                return await _context.Employees
                    .AsNoTracking()
                    .Include(e => e.JobTitle)
                    .Include(e => e.Phones)  // ✅ Incluir telefones
                    .Where(e => e.JobTitle != null)
                    .Select(e => e.JobTitle.Name)
                    .Distinct()
                    .OrderBy(jobTitle => jobTitle)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
