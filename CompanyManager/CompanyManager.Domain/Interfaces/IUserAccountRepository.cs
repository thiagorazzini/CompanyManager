using CompanyManager.Domain.Entities;

namespace CompanyManager.Domain.Interfaces
{
    public interface IUserAccountRepository
    {
        // Métodos de leitura
        Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserAccount>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<UserAccount>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<UserAccount>> GetInactiveAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<UserAccount>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserAccount>> GetLockedAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<UserAccount>> GetByPermissionAsync(string permission, CancellationToken cancellationToken = default);

        // Métodos de verificação de existência
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

        // Métodos de CRUD
        Task AddAsync(UserAccount account, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserAccount user, CancellationToken cancellationToken = default);
        Task SaveAsync(UserAccount account, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        // Métodos de gerenciamento de conta
        Task UpdateSecurityStampAsync(Guid id, Guid newSecurityStamp, CancellationToken cancellationToken = default);
        Task UpdateLastLoginAsync(Guid id, CancellationToken cancellationToken = default);
        Task IncrementFailedLoginAttemptsAsync(Guid id, CancellationToken cancellationToken = default);
        Task LockAccountAsync(Guid id, DateTime lockoutEnd, CancellationToken cancellationToken = default);
        Task UnlockAccountAsync(Guid id, CancellationToken cancellationToken = default);

        // Métodos de consulta em lote
        Task<IEnumerable<UserAccount>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default);
        Task<int> GetLockedCountAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<UserAccount>> GetUsersByLastLoginDateAsync(DateTime fromDate, CancellationToken cancellationToken = default);
    }
}
