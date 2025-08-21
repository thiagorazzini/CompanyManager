using CompanyManager.Domain.Entities;
using CompanyManager.Domain.AccessControl;

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
        Task<bool> IsLockedOutAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> IsTwoFactorEnabledAsync(Guid id, CancellationToken cancellationToken = default);
        Task<string?> GetTwoFactorSecretAsync(Guid id, CancellationToken cancellationToken = default);
        Task<DateTime> GetPasswordChangedAtAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Guid> GetSecurityStampAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> GetAccessFailedCountAsync(Guid id, CancellationToken cancellationToken = default);
        Task<DateTime?> GetLockoutEndUtcAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Guid> GetEmployeeIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Guid> GetRoleIdAsync(Guid id, CancellationToken cancellationToken = default);

        // Métodos de validação de permissões
        Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetAllPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsSuperUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<HierarchicalRole> GetRoleLevelAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
