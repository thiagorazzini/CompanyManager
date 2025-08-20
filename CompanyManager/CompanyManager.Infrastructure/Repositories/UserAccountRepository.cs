using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
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
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);
        }

        public async Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _context.UserAccounts
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserName == normalizedEmail && u.IsActive, cancellationToken);
        }

        public async Task<UserAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(normalizedEmail))
                return null;

            var normalized = normalizedEmail.Trim().ToLowerInvariant();
            return await _context.UserAccounts
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserName == normalized && u.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .Include(u => u.Roles)
                .Where(u => u.IsActive)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .Include(u => u.Roles)
                .Where(u => u.IsActive && !u.IsLockedOut)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetInactiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .Include(u => u.Roles)
                .Where(u => !u.IsActive)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return Enumerable.Empty<UserAccount>();

            return await _context.UserAccounts
                .Include(u => u.Roles)
                .Where(u => u.IsActive && u.Roles.Any(r => r.Name == roleName))
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetLockedAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .Include(u => u.Roles)
                .Where(u => u.IsLockedOut && u.IsActive)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetByPermissionAsync(string permission, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return Enumerable.Empty<UserAccount>();

            return await _context.UserAccounts
                .Include(u => u.Roles)
                .Where(u => u.IsActive && u.Roles.Any(r => r.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase)))
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        // Métodos de verificação de existência
        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .AsNoTracking()
                .AnyAsync(u => u.Id == id && u.IsActive, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _context.UserAccounts
                .AsNoTracking()
                .AnyAsync(u => u.UserName == normalizedEmail && u.IsActive, cancellationToken);
        }

        // Métodos de CRUD
        public async Task AddAsync(UserAccount account, CancellationToken cancellationToken = default)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));

            _context.UserAccounts.Add(account);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserAccount user, CancellationToken cancellationToken = default)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            _context.UserAccounts.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveAsync(UserAccount account, CancellationToken cancellationToken = default)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));

            if (account.Id == Guid.Empty)
            {
                await AddAsync(account, cancellationToken);
            }
            else
            {
                await UpdateAsync(account, cancellationToken);
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userAccount = await GetByIdAsync(id, cancellationToken);
            if (userAccount != null)
            {
                userAccount.Deactivate();
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        // Métodos de gerenciamento de conta
        public async Task UpdateSecurityStampAsync(Guid id, Guid newSecurityStamp, CancellationToken cancellationToken = default)
        {
            var userAccount = await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);
                
            if (userAccount != null)
            {
                // Usar ChangePassword com hash atual para atualizar apenas SecurityStamp
                // Isso é um workaround - idealmente a entidade deveria ter um método SetSecurityStamp
                var currentHash = userAccount.PasswordHash;
                userAccount.ChangePassword(currentHash);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task UpdateLastLoginAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userAccount = await GetByIdAsync(id, cancellationToken);
            if (userAccount != null)
            {
                userAccount.ResetFailuresAfterSuccessfulLogin();
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task IncrementFailedLoginAttemptsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userAccount = await GetByIdAsync(id, cancellationToken);
            if (userAccount != null)
            {
                userAccount.RecordFailedLoginAttempt(5, TimeSpan.FromMinutes(15)); // 5 tentativas, bloqueio por 15 min
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task LockAccountAsync(Guid id, DateTime lockoutEnd, CancellationToken cancellationToken = default)
        {
            var userAccount = await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);
                
            if (userAccount != null)
            {
                // Simular bloqueio usando RecordFailedLoginAttempt com muitas tentativas
                // Isso é um workaround - idealmente a entidade deveria ter um método LockUntil
                var maxAttempts = 1; // Forçar bloqueio imediato
                var lockoutDuration = lockoutEnd - DateTime.UtcNow;
                if (lockoutDuration > TimeSpan.Zero)
                {
                    userAccount.RecordFailedLoginAttempt(maxAttempts, lockoutDuration);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        public async Task UnlockAccountAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userAccount = await GetByIdAsync(id, cancellationToken);
            if (userAccount != null)
            {
                userAccount.UnlockNow();
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        // Métodos de consulta em lote
        public async Task<IEnumerable<UserAccount>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any())
                return Enumerable.Empty<UserAccount>();

            return await _context.UserAccounts
                .Include(u => u.Roles)
                .Where(u => ids.Contains(u.Id) && u.IsActive)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .AsNoTracking()
                .CountAsync(u => u.IsActive && !u.IsLockedOut, cancellationToken);
        }

        public async Task<int> GetLockedCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserAccounts
                .AsNoTracking()
                .CountAsync(u => u.IsLockedOut && u.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<UserAccount>> GetUsersByLastLoginDateAsync(DateTime fromDate, CancellationToken cancellationToken = default)
        {
            // A entidade UserAccount não tem campo LastLoginAt
            // Retornar usuários ativos ordenados por data de criação como alternativa
            // Nota: Este método pode precisar ser removido ou a entidade atualizada
            return await _context.UserAccounts
                .Include(u => u.Roles)
                .Where(u => u.IsActive && u.CreatedAt >= fromDate)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
