using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Common;

namespace CompanyManager.Domain.Entities
{
    public sealed class UserAccount : BaseEntity
    {
        // Credentials / identity
        public string UserName { get; private set; } = string.Empty; // normalized (trim + lower)
        public string PasswordHash { get; private set; } = string.Empty;
        public Guid SecurityStamp { get; private set; } = Guid.NewGuid();
        public DateTime PasswordChangedAt { get; private set; } = DateTime.UtcNow;
        private readonly List<Role> _roles = new();
        public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
        // State
        public bool IsActive { get; private set; } = true;

        // Lockout
        public int AccessFailedCount { get; private set; }
        public DateTime? LockoutEndUtc { get; private set; }
        public bool IsLockedOut => LockoutEndUtc.HasValue && LockoutEndUtc > DateTime.UtcNow;

        // Two-Factor
        public bool TwoFactorEnabled { get; private set; }
        public string? TwoFactorSecret { get; private set; }

        // Link to Employee
        public Guid EmployeeId { get; private set; }

        // EF constructor
        private UserAccount() { }

        private UserAccount(string userName, string passwordHash, Guid employeeId)
        {
            SetUserName(userName);
            SetPasswordHash(passwordHash);
            if (employeeId == Guid.Empty)
                throw new ArgumentException("Invalid employee id", nameof(employeeId));
            EmployeeId = employeeId;
        }

        public static UserAccount Create(string userName, string passwordHash, Guid employeeId)
            => new(userName, passwordHash, employeeId);

        public void SetUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Username cannot be null or empty", nameof(userName));

            UserName = userName.Trim().ToLowerInvariant();
            UpdateModifiedAt();
        }

        /// <summary>
        /// Receives the already-hashed password (hashing is external).
        /// Updates PasswordChangedAt, resets failures/lockout and renews the SecurityStamp.
        /// </summary>
        public void SetPasswordHash(string newHash)
        {
            if (string.IsNullOrWhiteSpace(newHash))
                throw new ArgumentException("Password hash cannot be null or empty", nameof(newHash));

            PasswordHash = newHash;
            PasswordChangedAt = DateTime.UtcNow;
            SecurityStamp = Guid.NewGuid();
            AccessFailedCount = 0;
            LockoutEndUtc = null;
            UpdateModifiedAt();
        }

        public void RecordFailedLoginAttempt(int maxAttempts, TimeSpan lockoutFor)
        {
            if (!IsActive) return;
            AccessFailedCount++;
            if (AccessFailedCount >= maxAttempts)
                LockoutEndUtc = DateTime.UtcNow.Add(lockoutFor);
            UpdateModifiedAt();
        }

        public void ResetFailuresAfterSuccessfulLogin()
        {
            AccessFailedCount = 0;
            LockoutEndUtc = null;
            UpdateModifiedAt();
        }

        public void UnlockNow()
        {
            LockoutEndUtc = null;
            AccessFailedCount = 0;
            UpdateModifiedAt();
        }

        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                UpdateModifiedAt();
            }
        }

        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                UpdateModifiedAt();
            }
        }

        public void EnableTwoFactor(string encryptedSecret)
        {
            if (string.IsNullOrWhiteSpace(encryptedSecret))
                throw new ArgumentException("Invalid 2FA secret", nameof(encryptedSecret));

            TwoFactorEnabled = true;
            TwoFactorSecret = encryptedSecret;
            UpdateModifiedAt();
        }

        public void DisableTwoFactor()
        {
            TwoFactorEnabled = false;
            TwoFactorSecret = null;
            UpdateModifiedAt();
        }

        public void AddRole(Role role)
        {
            if (role is null) throw new ArgumentNullException(nameof(role));
            if (_roles.Any(r => r.Id == role.Id)) return; // idempotente
            _roles.Add(role);
            UpdateModifiedAt();
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Hash required.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            SecurityStamp = Guid.NewGuid();
            UpdateModifiedAt(); 
        }

        public void RemoveRole(Role role)
        {
            if (_roles.RemoveAll(r => r.Id == role.Id) > 0)
                UpdateModifiedAt();
        }

        public IEnumerable<string> GetAllPermissions() =>
            _roles.SelectMany(r => r.Permissions).Distinct(StringComparer.OrdinalIgnoreCase);

        public bool HasPermission(string permission) =>
            _roles.Any(r => r.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase));

        /// <summary>
        /// Verifica se o usuário pode criar funcionários com o role especificado
        /// </summary>
        /// <param name="targetRole">Role que está sendo criado</param>
        /// <returns>True se o usuário pode criar funcionários com o role especificado</returns>
        public bool CanCreateRole(HierarchicalRole targetRole)
        {
            // Usuário deve ter pelo menos um role
            if (!_roles.Any()) return false;
            
            // Verificar se algum dos roles do usuário pode criar o role especificado
            return _roles.Any(r => r.CanCreateRole(targetRole));
        }

        /// <summary>
        /// Obtém o nível hierárquico mais alto do usuário
        /// </summary>
        /// <returns>Nível hierárquico mais alto</returns>
        public HierarchicalRole GetHighestRoleLevel()
        {
            if (!_roles.Any()) return HierarchicalRole.Junior;
            
            return _roles.Max(r => r.Level);
        }
    }
}
