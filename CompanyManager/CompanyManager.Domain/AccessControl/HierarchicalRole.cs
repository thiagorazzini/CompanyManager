using System.ComponentModel;

namespace CompanyManager.Domain.AccessControl
{
    /// <summary>
    /// Define os níveis hierárquicos dos roles na empresa
    /// </summary>
    public enum HierarchicalRole
    {
        [Description("Junior")]
        Junior = 1,
        
        [Description("Pleno")]
        Pleno = 2,
        
        [Description("Senior")]
        Senior = 3,
        
        [Description("Manager")]
        Manager = 4,
        
        [Description("Director")]
        Director = 5,
        
        [Description("SuperUser")]
        SuperUser = 999 // Nível especial com permissões totais
    }

    /// <summary>
    /// Extensões para o enum HierarchicalRole
    /// </summary>
    public static class HierarchicalRoleExtensions
    {
        /// <summary>
        /// Verifica se um role pode criar funcionários com outro role
        /// </summary>
        /// <param name="creatorRole">Role do usuário que está criando</param>
        /// <param name="targetRole">Role que está sendo criado</param>
        /// <returns>True se o usuário pode criar funcionários com o role especificado</returns>
        public static bool CanCreateRole(this HierarchicalRole creatorRole, HierarchicalRole targetRole)
        {
            // SuperUser pode criar qualquer role, incluindo outros SuperUsers
            if (creatorRole == HierarchicalRole.SuperUser)
                return true;
                
            // Usuário só pode criar funcionários com nível igual ou inferior
            // Lógica: números maiores = níveis mais altos, podem criar números menores = níveis mais baixos
            return (int)creatorRole >= (int)targetRole;
        }

        /// <summary>
        /// Verifica se um role é SuperUser
        /// </summary>
        /// <param name="role">Role para verificar</param>
        /// <returns>True se for SuperUser</returns>
        public static bool IsSuperUser(this HierarchicalRole role)
        {
            return role == HierarchicalRole.SuperUser;
        }

        /// <summary>
        /// Obtém o nome descritivo do role
        /// </summary>
        /// <param name="role">Role para obter o nome</param>
        /// <returns>Nome descritivo do role</returns>
        public static string GetDescription(this HierarchicalRole role)
        {
            var field = role.GetType().GetField(role.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            
            return attribute?.Description ?? role.ToString();
        }

        /// <summary>
        /// Obtém as permissões padrão para cada role
        /// </summary>
        /// <param name="role">Role para obter as permissões</param>
        /// <returns>Array de permissões</returns>
        public static string[] GetDefaultPermissions(this HierarchicalRole role)
        {
            return role switch
            {
                HierarchicalRole.Junior => new[] { "employees:read", "profile:read", "profile:update" },
                HierarchicalRole.Pleno => new[] { "employees:read", "profile:read", "profile:update", "projects:read" },
                HierarchicalRole.Senior => new[] { "employees:read", "profile:read", "profile:update", "projects:read", "projects:write", "mentoring:read" },
                HierarchicalRole.Manager => new[] { "employees:read", "employees:write", "profile:read", "profile:update", "projects:read", "projects:write", "mentoring:read", "mentoring:write", "departments:read" },
                HierarchicalRole.Director => new[] { "employees:read", "employees:write", "profile:read", "profile:update", "projects:read", "projects:write", "mentoring:read", "mentoring:write", "departments:read", "departments:write", "roles:read", "roles:write" },
                HierarchicalRole.SuperUser => new[] { 
                    "employees:read", "employees:write", "employees:delete", "employees:admin",
                    "profile:read", "profile:update", "profile:admin",
                    "projects:read", "projects:write", "projects:delete", "projects:admin",
                    "mentoring:read", "mentoring:write", "mentoring:delete", "mentoring:admin",
                    "departments:read", "departments:write", "departments:delete", "departments:admin",
                    "roles:read", "roles:write", "roles:delete", "roles:admin",
                    "users:read", "users:write", "users:delete", "users:admin",
                    "system:admin", "system:config", "system:audit"
                },
                _ => new[] { "profile:read" }
            };
        }
    }
}
