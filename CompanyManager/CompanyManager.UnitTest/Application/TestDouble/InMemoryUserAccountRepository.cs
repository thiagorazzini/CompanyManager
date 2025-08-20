using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.Entities;
using System.Linq;

namespace CompanyManager.UnitTest.Application.TestDouble
{
    internal sealed class InMemoryUserAccountRepository : IUserAccountRepository
    {
        private readonly Dictionary<Guid, UserAccount> _byId = new();
        private readonly Dictionary<string, UserAccount> _byEmail =
            new(StringComparer.OrdinalIgnoreCase);

        private static string Key(string email) =>
            (email ?? string.Empty).Trim().ToLowerInvariant();

        // Métodos de leitura
        public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            _byId.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken ct)
        {
            _byEmail.TryGetValue(Key(email), out var user);
            return Task.FromResult(user);
        }

        public Task<UserAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
        {
            _byEmail.TryGetValue(Key(normalizedEmail), out var user);
            return Task.FromResult(user);
        }

        public Task<IEnumerable<UserAccount>> GetAllAsync(CancellationToken ct)
        {
            return Task.FromResult(_byId.Values.AsEnumerable());
        }

        public Task<IEnumerable<UserAccount>> GetActiveAsync(CancellationToken ct)
        {
            return Task.FromResult(_byId.Values.Where(u => u.IsActive && !u.IsLockedOut).AsEnumerable());
        }

        public Task<IEnumerable<UserAccount>> GetInactiveAsync(CancellationToken ct)
        {
            return Task.FromResult(_byId.Values.Where(u => !u.IsActive).AsEnumerable());
        }

        public Task<IEnumerable<UserAccount>> GetByRoleAsync(string roleName, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return Task.FromResult(Enumerable.Empty<UserAccount>());

            var users = _byId.Values.Where(u => u.IsActive && u.Roles.Any(r => r.Name == roleName));
            return Task.FromResult(users.AsEnumerable());
        }

        public Task<IEnumerable<UserAccount>> GetLockedAsync(CancellationToken ct)
        {
            return Task.FromResult(_byId.Values.Where(u => u.IsLockedOut && u.IsActive).AsEnumerable());
        }

        public Task<IEnumerable<UserAccount>> GetByPermissionAsync(string permission, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return Task.FromResult(Enumerable.Empty<UserAccount>());

            var users = _byId.Values.Where(u => u.IsActive && u.Roles.Any(r => r.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase)));
            return Task.FromResult(users.AsEnumerable());
        }

        // Métodos de verificação de existência
        public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(_byId.ContainsKey(id));
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct)
        {
            return Task.FromResult(_byEmail.ContainsKey(Key(email)));
        }

        // Métodos de CRUD
        public Task AddAsync(UserAccount account, CancellationToken ct)
        {
            _byId[account.Id] = account;
            _byEmail[Key(account.UserName)] = account;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserAccount user, CancellationToken ct)
        {
            _byId[user.Id] = user;
            _byEmail[Key(user.UserName)] = user;
            return Task.CompletedTask;
        }

        public Task SaveAsync(UserAccount account, CancellationToken ct)
        {
            if (account.Id == Guid.Empty)
            {
                return AddAsync(account, ct);
            }
            else
            {
                return UpdateAsync(account, ct);
            }
        }

        public Task DeleteAsync(Guid id, CancellationToken ct)
        {
            if (_byId.TryGetValue(id, out var user))
            {
                user.Deactivate();
                _byId[id] = user;
                _byEmail[Key(user.UserName)] = user;
            }
            return Task.CompletedTask;
        }

        // Métodos de gerenciamento de conta
        public Task UpdateSecurityStampAsync(Guid id, Guid newSecurityStamp, CancellationToken ct)
        {
            if (_byId.TryGetValue(id, out var user))
            {
                var currentHash = user.PasswordHash;
                user.ChangePassword(currentHash);
                _byId[id] = user;
                _byEmail[Key(user.UserName)] = user;
            }
            return Task.CompletedTask;
        }

        public Task UpdateLastLoginAsync(Guid id, CancellationToken ct)
        {
            if (_byId.TryGetValue(id, out var user))
            {
                user.ResetFailuresAfterSuccessfulLogin();
                _byId[id] = user;
                _byEmail[Key(user.UserName)] = user;
            }
            return Task.CompletedTask;
        }

        public Task IncrementFailedLoginAttemptsAsync(Guid id, CancellationToken ct)
        {
            if (_byId.TryGetValue(id, out var user))
            {
                user.RecordFailedLoginAttempt(5, TimeSpan.FromMinutes(15));
                _byId[id] = user;
                _byEmail[Key(user.UserName)] = user;
            }
            return Task.CompletedTask;
        }

        public Task LockAccountAsync(Guid id, DateTime lockoutEnd, CancellationToken ct)
        {
            if (_byId.TryGetValue(id, out var user))
            {
                var maxAttempts = 1;
                var lockoutDuration = lockoutEnd - DateTime.UtcNow;
                if (lockoutDuration > TimeSpan.Zero)
                {
                    user.RecordFailedLoginAttempt(maxAttempts, lockoutDuration);
                    _byId[id] = user;
                    _byEmail[Key(user.UserName)] = user;
                }
            }
            return Task.CompletedTask;
        }

        public Task UnlockAccountAsync(Guid id, CancellationToken ct)
        {
            if (_byId.TryGetValue(id, out var user))
            {
                user.UnlockNow();
                _byId[id] = user;
                _byEmail[Key(user.UserName)] = user;
            }
            return Task.CompletedTask;
        }

        // Métodos de consulta em lote
        public Task<IEnumerable<UserAccount>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            if (ids == null || !ids.Any())
                return Task.FromResult(Enumerable.Empty<UserAccount>());

            var users = _byId.Values.Where(u => ids.Contains(u.Id) && u.IsActive);
            return Task.FromResult(users.AsEnumerable());
        }

        public Task<int> GetActiveCountAsync(CancellationToken ct)
        {
            return Task.FromResult(_byId.Values.Count(u => u.IsActive && !u.IsLockedOut));
        }

        public Task<int> GetLockedCountAsync(CancellationToken ct)
        {
            return Task.FromResult(_byId.Values.Count(u => u.IsLockedOut && u.IsActive));
        }

        public Task<IEnumerable<UserAccount>> GetUsersByLastLoginDateAsync(DateTime fromDate, CancellationToken ct)
        {
            var users = _byId.Values.Where(u => u.IsActive && u.CreatedAt >= fromDate);
            return Task.FromResult(users.AsEnumerable());
        }
    }
}
