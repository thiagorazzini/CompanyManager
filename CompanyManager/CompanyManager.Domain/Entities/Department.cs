using CompanyManager.Domain.Common;

namespace CompanyManager.Domain.Entities
{
    public sealed class Department : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Department() { }

        private Department(string name, string? description = null)
        {
            Name = ValidateName(name);
            Description = description?.Trim();
        }

        public static Department Create(string name, string? description = null) => new Department(name, description);

        public void Rename(string name)
        {
            var trimmed = ValidateName(name);
            if (trimmed == Name) return; 
            Name = trimmed;
            UpdateModifiedAt();
        }

        public void UpdateDescription(string? description)
        {
            var trimmed = description?.Trim();
            if (trimmed == Description) return;
            Description = trimmed;
            UpdateModifiedAt();
        }

        public void Activate()
        {
            if (IsActive) return; 
            IsActive = true;
            UpdateModifiedAt();
        }

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            UpdateModifiedAt();
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Invalid department name", nameof(name));

            var trimmed = name.Trim();
            if (trimmed.Length < 2)
                throw new ArgumentException("Invalid department name", nameof(name));

            return trimmed;
        }
    }
}
