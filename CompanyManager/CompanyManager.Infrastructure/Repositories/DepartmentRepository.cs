using CompanyManager.Domain.Common;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompanyManager.Infrastructure.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly CompanyContext _context;

    public DepartmentRepository(CompanyContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && d.IsActive, cancellationToken);
    }

    public async Task<Department?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        return await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Description != null &&
                d.Description.Equals(description, StringComparison.OrdinalIgnoreCase) &&
                d.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Department>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Department>> GetInactiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => !d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Department> Items, int Total)> SearchAsync(
        DepartmentFilter filter, PageRequest page, CancellationToken cancellationToken = default)
    {
        var query = _context.Departments.AsNoTracking().Where(d => d.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.NameContains))
        {
            var searchTerm = filter.NameContains.ToLowerInvariant();
            query = query.Where(d =>
                d.Name.ToLower().Contains(searchTerm) ||
                (d.Description != null && d.Description.ToLower().Contains(searchTerm)));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(d => d.Name)
            .Skip(page.Offset)
            .Take(page.Take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return await _context.Departments
            .AsNoTracking()
            .AnyAsync(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && d.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsByDescriptionAsync(string description, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(description))
            return false;

        return await _context.Departments
            .AsNoTracking()
            .AnyAsync(d => d.Description != null &&
                d.Description.Equals(description, StringComparison.OrdinalIgnoreCase) &&
                d.IsActive, cancellationToken);
    }

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        if (department is null)
            throw new ArgumentNullException(nameof(department));

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Department department, CancellationToken cancellationToken = default)
    {
        if (department is null)
            throw new ArgumentNullException(nameof(department));

        _context.Departments.Update(department);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await GetByIdAsync(id, cancellationToken);
        if (department != null)
        {
            department.Deactivate();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Department>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null || !ids.Any())
            return Enumerable.Empty<Department>();

        return await _context.Departments
            .AsNoTracking()
            .Where(d => ids.Contains(d.Id))
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetEmployeeCountAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .CountAsync(e => e.DepartmentId == departmentId, cancellationToken);
    }

    public async Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .CountAsync(d => d.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Department>> GetDepartmentsWithEmployeeCountAsync(CancellationToken cancellationToken = default)
    {
        var query = from d in _context.Departments
                    where d.IsActive
                    join e in _context.Employees on d.Id equals e.DepartmentId into employees
                    select new
                    {
                        Department = d,
                        EmployeeCount = employees.Count()
                    };

        var result = await query
            .AsNoTracking()
            .OrderBy(x => x.Department.Name)
            .ToListAsync(cancellationToken);

        return result.Select(x => x.Department);
    }
}
