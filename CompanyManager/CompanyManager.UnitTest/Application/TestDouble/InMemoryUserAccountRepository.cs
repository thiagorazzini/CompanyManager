using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CompanyManager.UnitTest.Application.TestDouble
{
    public class InMemoryUserAccountRepository : IUserAccountRepository
    {
        private readonly Dictionary<Guid, UserAccount> _byId = new();
        private readonly Dictionary<string, UserAccount> _byEmail = new();
        private readonly Dictionary<Guid, Role> _roles = new(); // Para simular as roles

        public InMemoryUserAccountRepository()
        {
        }

        public InMemoryUserAccountRepository(IEnumerable<UserAccount> users, IEnumerable<Role> roles)
        {
            foreach (var user in users)
            {
                _byId[user.Id] = user;
                _byEmail[Key(user.UserName)] = user;
            }
            
            foreach (var role in roles)
            {
                _roles[role.Id] = role;
            }
        }

        private static string Key(string email) => email.Trim().ToLowerInvariant();

        public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            return Task.FromResult(_byId.TryGetValue(id, out var user) && user.IsActive ? user : null);
        }

        public Task<UserAccount?> GetByEmailAsync(string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Task.FromResult<UserAccount?>(null);

            var key = Key(email);
            return Task.FromResult(_byEmail.TryGetValue(key, out var user) && user.IsActive ? user : null);
        }

        public Task<UserAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
        {
            return GetByEmailAsync(normalizedEmail, ct);
        }

        public Task<IEnumerable<UserAccount>> GetAllAsync(CancellationToken ct)
        {
            return Task.FromResult(_byId.Values.Where(u => u.IsActive).AsEnumerable());
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

            var users = _byId.Values.Where(u => u.IsActive && 
                _roles.TryGetValue(u.RoleId, out var role) && role.Name == roleName);
            return Task.FromResult(users.AsEnumerable());
        }

        public Task<IEnumerable<UserAccount>> GetLockedAsync(CancellationToken ct)
        {
            return Task.FromResult(_byId.Values.Where(u => u.IsActive && u.IsLockedOut).AsEnumerable());
        }

        public Task<IEnumerable<UserAccount>> GetByPermissionAsync(string permission, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return Task.FromResult(Enumerable.Empty<UserAccount>());

            var users = _byId.Values.Where(u => u.IsActive && 
                _roles.TryGetValue(u.RoleId, out var role) && role.Permissions.Contains(permission));
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
        public Task<bool> IsLockedOutAsync(Guid id, CancellationToken ct)
        {
            var user = _byId.TryGetValue(id, out var u) ? u : null;
            return Task.FromResult(user?.IsLockedOut ?? false);
        }

        public Task<bool> IsTwoFactorEnabledAsync(Guid id, CancellationToken ct)
        {
            var user = _byId.TryGetValue(id, out var u) ? u : null;
            return Task.FromResult(user?.TwoFactorEnabled ?? false);
        }

        public Task<string?> GetTwoFactorSecretAsync(Guid id, CancellationToken ct)
        {
            var user = _byId.TryGetValue(id, out var u) ? u : null;
            return Task.FromResult(user?.TwoFactorSecret);
        }

        public Task<DateTime> GetPasswordChangedAtAsync(Guid id, CancellationToken ct)
        {
            var user = _byId.TryGetValue(id, out var u) ? u : null;
            return Task.FromResult(user?.PasswordChangedAt ?? DateTime.MinValue);
        }

        public Task<Guid> GetSecurityStampAsync(Guid id, CancellationToken ct)
        {
            var user = _byId.TryGetValue(id, out var u) ? u : null;
            return Task.FromResult(user?.SecurityStamp ?? Guid.Empty);
        }

        public Task<int> GetAccessFailedCountAsync(Guid id, CancellationToken ct)
        {
            var user = _byId.TryGetValue(id, out var u) ? u : null;
            return Task.FromResult(user?.AccessFailedCount ?? 0);
        }

        public Task<DateTime?> GetLockoutEndUtcAsync(Guid id, CancellationToken ct)
        {
            var user = _byId.TryGetValue(id, out var u) ? u : null;
            return Task.FromResult(user?.LockoutEndUtc);
        }

        public Task<Guid> GetEmployeeIdAsync(Guid id, CancellationToken ct)
        {
            var user = _byId.TryGetValue(id, out var u) ? u : null;
            return Task.FromResult(user?.EmployeeId ?? Guid.Empty);
        }

        public Task<Guid> GetRoleIdAsync(Guid id, CancellationToken ct)
        {
            var user = _byId.TryGetValue(id, out var u) ? u : null;
            return Task.FromResult(user?.RoleId ?? Guid.Empty);
        }

        // Métodos de validação de permissões
        public Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct)
        {
            var user = _byId.TryGetValue(userId, out var u) ? u : null;
            if (user == null || !user.IsActive) return Task.FromResult(false);

            if (!_roles.TryGetValue(user.RoleId, out var role)) return Task.FromResult(false);

            // SuperUser tem todas as permissões
            if (role.IsSuperUser()) return Task.FromResult(true);

            return Task.FromResult(role.HasPermission(permission));
        }

        public Task<IEnumerable<string>> GetAllPermissionsAsync(Guid userId, CancellationToken ct)
        {
            var user = _byId.TryGetValue(userId, out var u) ? u : null;
            if (user == null || !user.IsActive) return Task.FromResult(Enumerable.Empty<string>());

            if (!_roles.TryGetValue(user.RoleId, out var role)) return Task.FromResult(Enumerable.Empty<string>());

            // SuperUser tem todas as permissões
            if (role.IsSuperUser())
            {
                var allPermissions = _roles.Values.SelectMany(r => r.Permissions).Distinct();
                return Task.FromResult(allPermissions);
            }

            return Task.FromResult(role.Permissions.AsEnumerable());
        }

        public Task<bool> IsSuperUserAsync(Guid userId, CancellationToken ct)
        {
            var user = _byId.TryGetValue(userId, out var u) ? u : null;
            if (user == null || !user.IsActive) return Task.FromResult(false);

            if (!_roles.TryGetValue(user.RoleId, out var role)) return Task.FromResult(false);

            return Task.FromResult(role.IsSuperUser());
        }

        public Task<HierarchicalRole> GetRoleLevelAsync(Guid userId, CancellationToken ct)
        {
            var user = _byId.TryGetValue(userId, out var u) ? u : null;
            if (user == null || !user.IsActive) return Task.FromResult(HierarchicalRole.Junior);

            if (!_roles.TryGetValue(user.RoleId, out var role)) return Task.FromResult(HierarchicalRole.Junior);

            return Task.FromResult(role.Level);
        }

        // Métodos obsoletos - mantidos para compatibilidade temporária
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
                user.ResetAccessFailedCount();
                _byId[id] = user;
                _byEmail[Key(user.UserName)] = user;
            }
            return Task.CompletedTask;
        }

        public Task IncrementFailedLoginAttemptsAsync(Guid id, CancellationToken ct)
        {
            if (_byId.TryGetValue(id, out var user))
            {
                user.IncrementAccessFailedCount();
                _byId[id] = user;
                _byEmail[Key(user.UserName)] = user;
            }
            return Task.CompletedTask;
        }

        public Task LockAccountAsync(Guid id, DateTime lockoutEnd, CancellationToken ct)
        {
            if (_byId.TryGetValue(id, out var user))
            {
                user.Lockout(lockoutEnd);
                _byId[id] = user;
                _byEmail[Key(user.UserName)] = user;
            }
            return Task.CompletedTask;
        }

        public Task UnlockAccountAsync(Guid id, CancellationToken ct)
        {
            if (_byId.TryGetValue(id, out var user))
            {
                user.Unlock();
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
            // Como não temos mais LastLoginAt, retornar usuários ativos ordenados por data de criação
            var users = _byId.Values.Where(u => u.IsActive && u.CreatedAt >= fromDate)
                                   .OrderByDescending(u => u.CreatedAt);
            return Task.FromResult(users.AsEnumerable());
        }
    }
}
