using CompanyManager.Domain.Common;
using CompanyManager.Domain.ValueObjects;

namespace CompanyManager.Domain.Entities;

public sealed class Employee : BaseEntity
{
    // ---- State ----
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    public Email Email { get; private set; } = null!;
    public DocumentNumber DocumentNumber { get; private set; } = null!;
    public DateOfBirth DateOfBirth { get; private set; } = default!;

    private readonly List<PhoneNumber> _phones = new();
    public IReadOnlyCollection<PhoneNumber> Phones => _phones.AsReadOnly();

    public string JobTitle { get; private set; } = string.Empty;
    private static string ValidateJobTitle(string value) =>
    string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    public Guid DepartmentId { get; private set; }
    public Department? Department { get; private set; } // opcional (navegação EF)
    
    // ---- Hierarchy ----
    public Guid? ManagerId { get; private set; }
    public Employee? Manager { get; private set; } // opcional (navegação EF)
    private readonly List<Employee> _subordinates = new();
    public IReadOnlyCollection<Employee> Subordinates => _subordinates.AsReadOnly();
    
    public string FullName => $"{FirstName} {LastName}";
    public bool HasManager => ManagerId.HasValue;
    public int HierarchyLevel => CalculateHierarchyLevel();

    // ---- EF ctor ----
    private Employee() { }

    private Employee(
        string firstName,
        string lastName,
        Email email,
        DocumentNumber documentNumber,
        DateOfBirth dateOfBirth,
        IEnumerable<PhoneNumber> phones,
       string jobTitle,
        Guid departmentId,
        Guid? managerId = null)
    {
        // nomes (sem tocar UpdatedAt)
        FirstName = ValidateFirstName(firstName);
        LastName = ValidateLastName(lastName);

        Email = email ?? throw new ArgumentNullException(nameof(email));
        DocumentNumber = documentNumber ?? throw new ArgumentNullException(nameof(documentNumber));
        DateOfBirth = dateOfBirth; // VO já garante não-futuro

        if (phones is null) throw new ArgumentNullException(nameof(phones));
        var list = phones.ToList();
        if (list.Count == 0)
            throw new ArgumentException("At least one phone is required.", nameof(phones));

        // adiciona sem alterar UpdatedAt
        foreach (var p in list)
            AddPhoneInternal(p);

        JobTitle = ValidateJobTitle(jobTitle);

        if (departmentId == Guid.Empty)
            throw new ArgumentException("Invalid department id.", nameof(departmentId));
        DepartmentId = departmentId;

        // Manager assignment
        if (managerId.HasValue)
        {
            if (managerId.Value == Guid.Empty)
                throw new ArgumentException("Invalid manager id.", nameof(managerId));
            ManagerId = managerId.Value;
        }

        // CreatedAt vem da BaseEntity; UpdatedAt permanece nulo na criação
    }

    public static Employee Create(
        string firstName,
        string lastName,
        Email email,
        DocumentNumber documentNumber,
        DateOfBirth dateOfBirth,
        IEnumerable<PhoneNumber> phones,
        string? jobTitle,
        Guid departmentId,
        Guid? managerId = null)
        => new(firstName, lastName, email, documentNumber, dateOfBirth, phones, jobTitle ?? string.Empty, departmentId, managerId);
    
    // ---- Hierarchy Methods ----
    
    /// <summary>
    /// Assigns a manager to this employee
    /// </summary>
    /// <param name="managerId">The ID of the manager to assign</param>
    /// <exception cref="ArgumentException">Thrown when managerId is empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when trying to assign the same manager or creating circular references</exception>
    public void AssignManager(Guid managerId)
    {
        if (managerId == Guid.Empty)
            throw new ArgumentException("Manager ID cannot be empty.", nameof(managerId));

        if (ManagerId == managerId)
            throw new InvalidOperationException("Employee already has this manager assigned.");

        // Prevent self-assignment
        if (managerId == Id)
            throw new InvalidOperationException("Employee cannot be their own manager.");

        // Prevent circular references (this would be validated at the service level)
        // but we can check immediate circular reference
        if (HasSubordinate(managerId))
            throw new InvalidOperationException("Cannot assign a subordinate as manager (circular reference).");

        // Remove from previous manager's subordinates if any
        if (ManagerId.HasValue)
        {
            // This would be handled by the repository/service layer
            // For now, we just update the reference
        }

        ManagerId = managerId;
        UpdateModifiedAt();
    }

    /// <summary>
    /// Removes the current manager from this employee
    /// </summary>
    public void RemoveManager()
    {
        if (!HasManager) return;

        // Remove from previous manager's subordinates if any
        if (ManagerId.HasValue)
        {
            // This would be handled by the repository/service layer
            // For now, we just update the reference
        }

        ManagerId = null;
        UpdateModifiedAt();
    }

    /// <summary>
    /// Adds a subordinate to this employee
    /// </summary>
    /// <param name="subordinate">The subordinate employee to add</param>
    internal void AddSubordinate(Employee subordinate)
    {
        if (subordinate is null) return;
        if (_subordinates.Any(s => s.Id == subordinate.Id)) return;

        _subordinates.Add(subordinate);
    }

    /// <summary>
    /// Removes a subordinate from this employee
    /// </summary>
    /// <param name="subordinateId">The ID of the subordinate to remove</param>
    internal void RemoveSubordinate(Guid subordinateId)
    {
        var subordinate = _subordinates.FirstOrDefault(s => s.Id == subordinateId);
        if (subordinate != null)
        {
            _subordinates.Remove(subordinate);
        }
    }

    /// <summary>
    /// Checks if this employee has a specific subordinate
    /// </summary>
    /// <param name="subordinateId">The ID of the subordinate to check</param>
    /// <returns>True if the employee has the specified subordinate</returns>
    public bool HasSubordinate(Guid subordinateId)
    {
        return _subordinates.Any(s => s.Id == subordinateId);
    }

    /// <summary>
    /// Calculates the hierarchy level of this employee
    /// </summary>
    /// <returns>The hierarchy level (0 = top level, 1 = has manager, 2 = manager has manager, etc.)</returns>
    private int CalculateHierarchyLevel()
    {
        if (!HasManager) return 0;
        
        // For now, we'll use a simple calculation
        // In a real scenario, you might want to traverse the hierarchy tree
        // This is a placeholder implementation
        return 1;
    }

    // ---- Mutators (atualizam UpdatedAt quando há mudança real) ----

    public void ChangeName(string firstName, string lastName)
    {
        var newFirst = ValidateFirstName(firstName);
        var newLast = ValidateLastName(lastName);

        if (newFirst == FirstName && newLast == LastName) return;

        FirstName = newFirst;
        LastName = newLast;
        UpdateModifiedAt();
    }

    public void ChangeJobTitle(string title)
    {
        var trimmed = ValidateJobTitle(title);
        if (trimmed == JobTitle) return;
        JobTitle = trimmed;
        UpdateModifiedAt();
    }

    public void ChangeDepartment(Guid departmentId)
    {
        if (departmentId == Guid.Empty)
            throw new ArgumentException("Invalid department id.", nameof(departmentId));

        if (DepartmentId == departmentId) return;

        DepartmentId = departmentId;
        UpdateModifiedAt();
    }

    public void ChangeEmail(Email newEmail)
    {
        if (newEmail is null) throw new ArgumentNullException(nameof(newEmail));
        if (Email.Equals(newEmail)) return; // usa igualdade do VO
        Email = newEmail;
        UpdateModifiedAt();
    }

    public void ChangeDocument(DocumentNumber newDocument)
    {
        if (newDocument is null) throw new ArgumentNullException(nameof(newDocument));
        if (DocumentNumber == newDocument) return; 
        DocumentNumber = newDocument;
        UpdateModifiedAt();
    }

    public void AddPhone(PhoneNumber phone)
    {
        if (phone is null) throw new ArgumentNullException(nameof(phone));
        if (_phones.Any(p => p == phone))
            throw new InvalidOperationException("Cannot add duplicate phone.");

        _phones.Add(phone);
        UpdateModifiedAt();
    }

    public void RemovePhone(PhoneNumber phone)
    {
        if (phone is null) return;

        // não permitir ficar sem telefone
        if (_phones.Count <= 1 && _phones.Contains(phone))
            throw new InvalidOperationException("Employee must have at least one phone.");

        var removed = _phones.Remove(phone);
        if (removed)
            UpdateModifiedAt();
    }

    // ---- Helpers ----

    private void AddPhoneInternal(PhoneNumber phone)
    {
        if (phone is null) throw new ArgumentNullException(nameof(phone));
        if (_phones.Any(p => p == phone))
            throw new InvalidOperationException("Cannot add duplicate phone.");
        _phones.Add(phone);
        // sem UpdateModifiedAt() durante a criação
    }

    private static string ValidateFirstName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invalid first name.", nameof(value));
        var trimmed = value.Trim();
        if (trimmed.Length < 2)
            throw new ArgumentException("Invalid first name.", nameof(value));
        return trimmed;
    }

    private static string ValidateLastName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invalid last name.", nameof(value));
        var trimmed = value.Trim();
        if (trimmed.Length < 2)
            throw new ArgumentException("Invalid last name.", nameof(value));
        return trimmed;
    }
}