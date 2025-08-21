using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompanyManager.Infrastructure.Repositories;

public class JobTitleRepository : IJobTitleRepository
{
    private readonly CompanyContext _context;

    public JobTitleRepository(CompanyContext context)
    {
        _context = context;
    }

    public async Task<JobTitle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JobTitles
            .FirstOrDefaultAsync(jt => jt.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<JobTitle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JobTitles
            .OrderBy(jt => jt.HierarchyLevel)
            .ThenBy(jt => jt.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JobTitle>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JobTitles
            .Where(jt => jt.IsActive)
            .OrderBy(jt => jt.HierarchyLevel)
            .ThenBy(jt => jt.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<JobTitle?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.JobTitles
            .FirstOrDefaultAsync(jt => jt.Name.ToLower() == name.Trim().ToLower(), cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.JobTitles
                .AnyAsync(jt => jt.Id == id, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Se a operação foi cancelada, tentar uma abordagem mais simples
            return await _context.JobTitles
                .CountAsync(jt => jt.Id == id, cancellationToken) > 0;
        }
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return await _context.JobTitles
            .AnyAsync(jt => jt.Name.ToLower() == name.Trim().ToLower(), cancellationToken);
    }

    public async Task AddAsync(JobTitle jobTitle, CancellationToken cancellationToken = default)
    {
        await _context.JobTitles.AddAsync(jobTitle, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(JobTitle jobTitle, CancellationToken cancellationToken = default)
    {
        _context.JobTitles.Update(jobTitle);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var jobTitle = await GetByIdAsync(id, cancellationToken);
        if (jobTitle != null)
        {
            // Verificar se há funcionários usando este cargo
            var hasEmployees = await _context.Employees
                .AnyAsync(e => e.JobTitleId == id, cancellationToken);

            if (hasEmployees)
            {
                // Se há funcionários, apenas desativar o cargo
                jobTitle.Deactivate();
                await UpdateAsync(jobTitle, cancellationToken);
            }
            else
            {
                // Se não há funcionários, pode deletar
                _context.JobTitles.Update(jobTitle);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
