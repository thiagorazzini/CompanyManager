using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;

namespace CompanyManager.UnitTest.Application.TestDouble
{
    public class StubJobTitleRepository : IJobTitleRepository
    {
        public Task<JobTitle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // Criar um JobTitle fake para os testes com nível adequado
            // Usar um ID fixo para garantir consistência nos testes
            var jobTitle = JobTitle.Create("Developer", 4, "Software Developer");
            return Task.FromResult<JobTitle?>(jobTitle);
        }

        public Task<IEnumerable<JobTitle>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<JobTitle>>(Array.Empty<JobTitle>());
        }

        public Task<IEnumerable<JobTitle>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<JobTitle>>(Array.Empty<JobTitle>());
        }

        public Task<JobTitle?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<JobTitle?>(null);
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true); // Sempre retorna true para os testes
        }

        public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task AddAsync(JobTitle jobTitle, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(JobTitle jobTitle, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
