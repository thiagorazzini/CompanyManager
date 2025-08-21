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
        
        // Role - agora apenas um ID em vez de coleção
        public Guid RoleId { get; private set; }
        
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
        
        // Link to JobTitle
        public Guid JobTitleId { get; private set; }

        // EF constructor
        private UserAccount() { }

        private UserAccount(string userName, string passwordHash, Guid employeeId, Guid roleId, Guid jobTitleId)
        {
            SetUserName(userName);
            SetPasswordHash(passwordHash);
            if (employeeId == Guid.Empty)
                throw new ArgumentException("Invalid employee id", nameof(employeeId));
            if (roleId == Guid.Empty)
                throw new ArgumentException("Invalid role id", nameof(roleId));
            if (jobTitleId == Guid.Empty)
                throw new ArgumentException("Invalid job title id", nameof(jobTitleId));
            EmployeeId = employeeId;
            RoleId = roleId;
            JobTitleId = jobTitleId;
        }

        public static UserAccount Create(string userName, string passwordHash, Guid employeeId, Guid roleId, Guid jobTitleId)
            => new(userName, passwordHash, employeeId, roleId, jobTitleId);

        public void SetUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("Invalid username", nameof(userName));

            UserName = userName.Trim().ToLowerInvariant();
            UpdateModifiedAt();
        }

        public void SetPasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Hash required.", nameof(passwordHash));

            PasswordHash = passwordHash;
            PasswordChangedAt = DateTime.UtcNow;
            UpdateModifiedAt();
        }

        public void SetRole(Guid roleId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("Invalid role id", nameof(roleId));

            RoleId = roleId;
            UpdateModifiedAt();
        }

        public void SetJobTitle(Guid jobTitleId)
        {
            if (jobTitleId == Guid.Empty)
                throw new ArgumentException("Invalid job title id", nameof(jobTitleId));

            JobTitleId = jobTitleId;
            UpdateModifiedAt();
        }

        public void Activate()
        {
            IsActive = true;
            UpdateModifiedAt();
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdateModifiedAt();
        }

        public void IncrementAccessFailedCount()
        {
            AccessFailedCount++;
            UpdateModifiedAt();
        }

        public void ResetAccessFailedCount()
        {
            AccessFailedCount = 0;
            UpdateModifiedAt();
        }

        public void Lockout(DateTime lockoutEnd)
        {
            if (lockoutEnd <= DateTime.UtcNow)
                throw new ArgumentException("Lockout end must be in the future", nameof(lockoutEnd));

            LockoutEndUtc = lockoutEnd;
            UpdateModifiedAt();
        }

        public void Unlock()
        {
            LockoutEndUtc = null;
            AccessFailedCount = 0;
            UpdateModifiedAt();
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

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Hash required.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            SecurityStamp = Guid.NewGuid();
            UpdateModifiedAt(); 
        }

        // Métodos de validação de permissões agora precisam receber a Role como parâmetro
        // já que não temos mais acesso direto à coleção de roles
        
        /// <summary>
        /// Verifica se o usuário pode criar funcionários com o role especificado
        /// </summary>
        /// <param name="userRole">Role do usuário atual</param>
        /// <param name="targetRole">Role que está sendo criado</param>
        /// <returns>True se o usuário pode criar funcionários com o role especificado</returns>
        public bool CanCreateRole(Role userRole, HierarchicalRole targetRole)
        {
            if (userRole is null) return false;
            return userRole.CanCreateRole(targetRole);
        }

        /// <summary>
        /// Verifica se o usuário é SuperUser
        /// </summary>
        /// <param name="userRole">Role do usuário atual</param>
        /// <returns>True se o usuário for SuperUser</returns>
        public bool IsSuperUser(Role userRole)
        {
            return userRole?.IsSuperUser() ?? false;
        }

        /// <summary>
        /// Obtém o nível hierárquico do usuário
        /// </summary>
        /// <param name="userRole">Role do usuário atual</param>
        /// <returns>Nível hierárquico</returns>
        public HierarchicalRole GetRoleLevel(Role userRole)
        {
            return userRole?.Level ?? HierarchicalRole.Junior;
        }

        /// <summary>
        /// Verifica se o usuário tem uma permissão específica
        /// SuperUser tem todas as permissões
        /// </summary>
        /// <param name="userRole">Role do usuário atual</param>
        /// <param name="permission">Permissão a verificar</param>
        /// <returns>True se tiver a permissão</returns>
        public bool HasPermission(Role userRole, string permission)
        {
            if (userRole?.IsSuperUser() == true)
                return true; // SuperUser tem todas as permissões
                
            return userRole?.HasPermission(permission) ?? false;
        }

        /// <summary>
        /// Verifica se o usuário pode modificar outro usuário
        /// SuperUser pode modificar qualquer usuário
        /// Usuários só podem modificar usuários com nível hierárquico inferior
        /// </summary>
        /// <param name="userRole">Role do usuário atual</param>
        /// <param name="targetUser">Usuário que está sendo modificado</param>
        /// <param name="targetUserRole">Role do usuário alvo</param>
        /// <returns>True se o usuário pode modificar o usuário alvo</returns>
        public bool CanModifyUser(Role userRole, UserAccount targetUser, Role targetUserRole)
        {
            if (targetUser is null || userRole is null || targetUserRole is null) return false;
            
            // SuperUser pode modificar qualquer usuário
            if (userRole.IsSuperUser()) return true;
            
            // Usuário só pode modificar usuários com nível hierárquico inferior
            return userRole.Level > targetUserRole.Level;
        }

        /// <summary>
        /// Verifica se o usuário pode modificar um departamento
        /// </summary>
        /// <param name="userRole">Role do usuário atual</param>
        /// <param name="department">Departamento que está sendo modificado</param>
        /// <returns>True se o usuário pode modificar o departamento</returns>
        public bool CanModifyDepartment(Role userRole, Department department)
        {
            if (userRole is null || department is null) return false;
            
            // SuperUser pode modificar qualquer departamento
            if (userRole.IsSuperUser()) return true;
            
            // Apenas Manager e Director podem modificar departamentos
            return userRole.Level >= HierarchicalRole.Manager;
        }

        /// <summary>
        /// Verifica se o usuário pode modificar um cargo
        /// </summary>
        /// <param name="userRole">Role do usuário atual</param>
        /// <param name="jobTitle">Cargo que está sendo modificado</param>
        /// <returns>True se o usuário pode modificar o cargo</returns>
        public bool CanModifyJobTitle(Role userRole, JobTitle jobTitle)
        {
            if (userRole is null || jobTitle is null) return false;
            
            // SuperUser pode modificar qualquer cargo
            if (userRole.IsSuperUser()) return true;
            
            // Apenas Manager e Director podem modificar cargos
            return userRole.Level >= HierarchicalRole.Manager;
        }

        /// <summary>
        /// Verifica se o usuário pode modificar um funcionário
        /// </summary>
        /// <param name="userRole">Role do usuário atual</param>
        /// <param name="employee">Funcionário que está sendo modificado</param>
        /// <param name="employeeJobTitle">Cargo do funcionário</param>
        /// <returns>True se o usuário pode modificar o funcionário</returns>
        public bool CanModifyEmployee(Role userRole, Employee employee, JobTitle employeeJobTitle)
        {
            if (userRole is null || employee is null || employeeJobTitle is null) return false;
            
            // SuperUser pode modificar qualquer funcionário
            if (userRole.IsSuperUser()) return true;
            
            // Apenas Manager e Director podem modificar funcionários
            return userRole.Level >= HierarchicalRole.Manager;
        }
    }
}
