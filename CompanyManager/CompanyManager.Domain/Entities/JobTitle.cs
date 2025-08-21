using CompanyManager.Domain.Common;

namespace CompanyManager.Domain.Entities;

public sealed class JobTitle : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public int HierarchyLevel { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    // Relacionamentos
    private readonly List<Employee> _employees = new();
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();

    // Construtor privado para EF
    private JobTitle() { }

    private JobTitle(string name, int hierarchyLevel, string description)
    {
        Name = ValidateName(name);
        HierarchyLevel = ValidateHierarchyLevel(hierarchyLevel);
        Description = ValidateDescription(description);
    }

    public static JobTitle Create(string name, int hierarchyLevel, string description)
        => new(name, hierarchyLevel, description);

    // Validações
    private static string ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Job title name cannot be empty.", nameof(value));
        
        var trimmed = value.Trim();
        if (trimmed.Length < 2)
            throw new ArgumentException("Job title name must have at least 2 characters.", nameof(value));
        
        return trimmed;
    }

    private static int ValidateHierarchyLevel(int value)
    {
        if (value < 1 || (value > 5 && value != 999))
            throw new ArgumentException("Hierarchy level must be between 1 and 5, or 999 for SuperUser.", nameof(value));
        
        return value;
    }

    private static string ValidateDescription(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    // Métodos de negócio
    public void Update(string name, int hierarchyLevel, string description)
    {
        Name = ValidateName(name);
        HierarchyLevel = ValidateHierarchyLevel(hierarchyLevel);
        Description = ValidateDescription(description);
        UpdateModifiedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateModifiedAt();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateModifiedAt();
    }

    // Métodos internos para relacionamentos
    internal void AddEmployee(Employee employee)
    {
        if (employee is null) return;
        if (_employees.Any(e => e.Id == employee.Id)) return;

        _employees.Add(employee);
    }

    internal void RemoveEmployee(Employee employee)
    {
        if (employee is null) return;
        _employees.Remove(employee);
    }

    // Propriedades computadas
    public bool IsTopLevel => HierarchyLevel == 1; // President
    public bool IsManagement => HierarchyLevel <= 3; // President, Director, Head
    public bool CanManage(JobTitle other) => HierarchyLevel < other.HierarchyLevel;
}

