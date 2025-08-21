using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.AccessControl;
using CompanyManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompanyManager.Infrastructure.Repositories
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly CompanyContext _context;

        public UserAccountRepository(CompanyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Métodos de leitura
        public async Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);
        }

        public async Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.UserName == normalizedEmail && u.IsActive, cancellationToken);
        }

        public async Task<UserAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(normalizedEmail))
                return null;

            var normalized = normalizedEmail.Trim().ToLowerInvariant();
            return await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.UserName == normalized && u.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .Where(u => u.IsActive)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .Where(u => u.IsActive && !u.IsLockedOut)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetInactiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .Where(u => !u.IsActive)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return Enumerable.Empty<UserAccount>();

            return await _context.UserAccounts
                .Where(u => u.IsActive)
                .Join(_context.Roles.Where(r => r.Name == roleName),
                      u => u.RoleId,
                      r => r.Id,
                      (u, r) => u)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetLockedAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .Where(u => u.IsActive && u.IsLockedOut)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetByPermissionAsync(string permission, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return Enumerable.Empty<UserAccount>();

            return await _context.UserAccounts
                .Where(u => u.IsActive)
                .Join(_context.Roles.Where(r => r.Permissions.Contains(permission)),
                      u => u.RoleId,
                      r => r.Id,
                      (u, r) => u)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        // Métodos de verificação de existência
        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .AnyAsync(u => u.Id == id && u.IsActive, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _context.UserAccounts
                .AnyAsync(u => u.UserName == normalizedEmail && u.IsActive, cancellationToken);
        }

        // Métodos de CRUD
        public async Task AddAsync(UserAccount account, CancellationToken cancellationToken = default)
        {
            if (account is null) throw new ArgumentNullException(nameof(account));
            await _context.UserAccounts.AddAsync(account, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserAccount user, CancellationToken cancellationToken = default)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            _context.UserAccounts.Update(user);
            await Task.CompletedTask;
        }

        public async Task SaveAsync(UserAccount account, CancellationToken cancellationToken = default)
        {
            if (account is null) throw new ArgumentNullException(nameof(account));
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            if (user != null)
            {
                user.Deactivate();
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        // Métodos de gerenciamento de conta
        public async Task<bool> IsLockedOutAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            return user?.IsLockedOut ?? false;
        }

        public async Task<bool> IsTwoFactorEnabledAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            return user?.TwoFactorEnabled ?? false;
        }

        public async Task<string?> GetTwoFactorSecretAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            return user?.TwoFactorSecret;
        }

        public async Task<DateTime> GetPasswordChangedAtAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            return user?.PasswordChangedAt ?? DateTime.MinValue;
        }

        public async Task<Guid> GetSecurityStampAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            return user?.SecurityStamp ?? Guid.Empty;
        }

        public async Task<int> GetAccessFailedCountAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            return user?.AccessFailedCount ?? 0;
        }

        public async Task<DateTime?> GetLockoutEndUtcAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            return user?.LockoutEndUtc;
        }

        public async Task<Guid> GetEmployeeIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            return user?.EmployeeId ?? Guid.Empty;
        }

        public async Task<Guid> GetRoleIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            return user?.RoleId ?? Guid.Empty;
        }

        // Métodos de validação de permissões
        public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
        {
            var user = await _context.UserAccounts
                .Where(u => u.Id == userId && u.IsActive)
                .Join(_context.Roles,
                      u => u.RoleId,
                      r => r.Id,
                      (u, r) => new { User = u, Role = r })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null) return false;

            // SuperUser tem todas as permissões
            if (user.Role.IsSuperUser()) return true;

            return user.Role.HasPermission(permission);
        }

        public async Task<IEnumerable<string>> GetAllPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.UserAccounts
                .Where(u => u.Id == userId && u.IsActive)
                .Join(_context.Roles,
                      u => u.RoleId,
                      r => r.Id,
                      (u, r) => new { User = u, Role = r })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null) return Enumerable.Empty<string>();

            // SuperUser tem todas as permissões
            if (user.Role.IsSuperUser())
            {
                // Retornar todas as permissões possíveis
                return await _context.Roles
                    .SelectMany(r => r.Permissions)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }

            return user.Role.Permissions;
        }

        public async Task<bool> IsSuperUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.UserAccounts
                .Where(u => u.Id == userId && u.IsActive)
                .Join(_context.Roles,
                      u => u.RoleId,
                      r => r.Id,
                      (u, r) => new { User = u, Role = r })
                .FirstOrDefaultAsync(cancellationToken);

            return user?.Role.IsSuperUser() ?? false;
        }

        public async Task<HierarchicalRole> GetRoleLevelAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.UserAccounts
                .Where(u => u.Id == userId && u.IsActive)
                .Join(_context.Roles,
                      u => u.RoleId,
                      r => r.Id,
                      (u, r) => new { User = u, Role = r })
                .FirstOrDefaultAsync(cancellationToken);

            return user?.Role.Level ?? HierarchicalRole.Junior;
        }
    }
}

