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
        Director = 5
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
            // Usuário só pode criar funcionários com nível igual ou inferior
            return (int)targetRole <= (int)creatorRole;
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
                _ => new[] { "profile:read" }
            };
        }
    }
}
