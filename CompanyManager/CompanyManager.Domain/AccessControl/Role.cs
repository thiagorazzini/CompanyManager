using CompanyManager.Domain.Common;

namespace CompanyManager.Domain.AccessControl
{
    public sealed class Role : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public HierarchicalRole Level { get; private set; }

        private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);
        public IReadOnlyCollection<string> Permissions => _permissions;

        private Role() { }

        public Role(string name, HierarchicalRole level, IEnumerable<string>? perms = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Invalid role name", nameof(name));
            Name = name.Trim();
            Level = level;
            
            // Se não foram fornecidas permissões, usar as padrão do nível
            var permissions = perms ?? level.GetDefaultPermissions();
            foreach (var p in permissions) AddPermission(p);
        }

        public Role(string name, IEnumerable<string>? perms = null) : this(name, HierarchicalRole.Junior, perms)
        {
        }

        public void AddPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission)) throw new ArgumentException("Invalid permission", nameof(permission));
            _permissions.Add(permission.Trim());
            UpdateModifiedAt();
        }

        public void RemovePermission(string permission)
        {
            if (_permissions.Remove(permission)) UpdateModifiedAt();
        }

        public void SetLevel(HierarchicalRole level)
        {
            Level = level;
            UpdateModifiedAt();
        }

        public bool CanCreateRole(HierarchicalRole targetLevel)
        {
            return Level.CanCreateRole(targetLevel);
        }
    }
}
