using CompanyManager.Domain.Entities;

namespace CompanyManager.Domain.Interfaces;

public interface IJobTitleRepository
{
    Task<JobTitle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobTitle>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<JobTitle>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<JobTitle?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(JobTitle jobTitle, CancellationToken cancellationToken = default);
    Task UpdateAsync(JobTitle jobTitle, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}



