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

    private readonly List<EmployeePhone> _phones = new();
    public IReadOnlyCollection<EmployeePhone> Phones => _phones.AsReadOnly();

    public Guid JobTitleId { get; private set; }
    public JobTitle? JobTitle { get; private set; } // opcional (navegação EF)
    private static Guid ValidateJobTitleId(Guid value) =>
        value == Guid.Empty ? throw new ArgumentException("Job title ID cannot be empty.", nameof(value)) : value;

    public Guid DepartmentId { get; private set; }
    public Department? Department { get; private set; } // opcional (navegação EF)
    
    // ---- Hierarchy ----
    // ManagerId removido - não há mais hierarquia de managers
    public string FullName => $"{FirstName} {LastName}";
    public bool HasManager => false; // Sempre false agora
    public int HierarchyLevel => 0; // Sempre 0 agora

    // ---- EF ctor ----
    private Employee() { }

    private Employee(
        string firstName,
        string lastName,
        Email email,
        DocumentNumber documentNumber,
        DateOfBirth dateOfBirth,
        IEnumerable<string> phoneNumbers,
        Guid jobTitleId,
        Guid departmentId)
    {
        // nomes (sem tocar UpdatedAt)
        FirstName = ValidateFirstName(firstName);
        LastName = ValidateLastName(lastName);

        Email = email ?? throw new ArgumentNullException(nameof(email));
        DocumentNumber = documentNumber ?? throw new ArgumentNullException(nameof(documentNumber));
        DateOfBirth = dateOfBirth; // VO já garante não-futuro

        if (phoneNumbers is null) throw new ArgumentNullException(nameof(phoneNumbers));
        var phoneList = phoneNumbers.ToList();
        if (phoneList.Count == 0)
            throw new ArgumentException("At least one phone is required.", nameof(phoneNumbers));

        // Criar telefones sem alterar UpdatedAt
        for (int i = 0; i < phoneList.Count; i++)
        {
            var phoneNumber = new PhoneNumber(phoneList[i], "BR");
            var employeePhone = EmployeePhone.Create(Id, phoneNumber, "Mobile", i == 0); // Primeiro telefone é principal
            AddPhoneInternal(employeePhone);
        }

        JobTitleId = ValidateJobTitleId(jobTitleId);

        if (departmentId == Guid.Empty)
            throw new ArgumentException("Invalid department id.", nameof(departmentId));
        DepartmentId = departmentId;
    }

    public static Employee Create(
        string firstName,
        string lastName,
        Email email,
        DocumentNumber documentNumber,
        DateOfBirth dateOfBirth,
        IEnumerable<string> phoneNumbers,
        Guid jobTitleId,
        Guid departmentId)
        => new(firstName, lastName, email, documentNumber, dateOfBirth, phoneNumbers, jobTitleId, departmentId);
    
    // ---- Phone Methods ----
    
    /// <summary>
    /// Adiciona um novo telefone ao funcionário
    /// </summary>
    public void AddPhone(string phoneNumber, string type = "Mobile", bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(phoneNumber));

        var phone = new PhoneNumber(phoneNumber, "BR");
        
        // Verificar duplicatas pelo número E164
        if (_phones.Any(p => p.PhoneNumber.E164 == phone.E164))
            throw new InvalidOperationException("Cannot add duplicate phone number.");

        // Se este telefone deve ser principal, remover a marcação dos outros
        if (isPrimary)
        {
            foreach (var existingPhone in _phones)
                existingPhone.SetAsPrimary(false);
        }

        var employeePhone = EmployeePhone.Create(Id, phone, type, isPrimary);
        _phones.Add(employeePhone);
        UpdateModifiedAt();
    }

    /// <summary>
    /// Remove um telefone do funcionário
    /// </summary>
    public void RemovePhone(Guid phoneId)
    {
        var phone = _phones.FirstOrDefault(p => p.Id == phoneId);
        if (phone is null) return;

        // não permitir ficar sem telefone
        if (_phones.Count <= 1)
            throw new InvalidOperationException("Employee must have at least one phone.");

        var removed = _phones.Remove(phone);
        if (removed)
            UpdateModifiedAt();
    }

    /// <summary>
    /// Atualiza todos os telefones do funcionário
    /// </summary>
    public void UpdatePhones(IEnumerable<string> phoneNumbers)
    {
        if (phoneNumbers is null) throw new ArgumentNullException(nameof(phoneNumbers));
        var phoneList = phoneNumbers.ToList();
        if (phoneList.Count == 0)
            throw new ArgumentException("At least one phone is required.", nameof(phoneNumbers));

        // Limpar telefones existentes
        _phones.Clear();

        // Adicionar novos telefones
        for (int i = 0; i < phoneList.Count; i++)
        {
            var phoneNumber = new PhoneNumber(phoneList[i], "BR");
            var employeePhone = EmployeePhone.Create(Id, phoneNumber, "Mobile", i == 0); // Primeiro é principal
            _phones.Add(employeePhone);
        }

        UpdateModifiedAt();
    }

    /// <summary>
    /// Define um telefone como principal
    /// </summary>
    public void SetPrimaryPhone(Guid phoneId)
    {
        var targetPhone = _phones.FirstOrDefault(p => p.Id == phoneId);
        if (targetPhone is null)
            throw new ArgumentException("Phone not found.", nameof(phoneId));

        // Remover marcação de principal de todos os telefones
        foreach (var phone in _phones)
            phone.SetAsPrimary(false);

        // Marcar o telefone alvo como principal
        targetPhone.SetAsPrimary(true);
        UpdateModifiedAt();
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

    public void ChangeJobTitle(Guid jobTitleId)
    {
        var validatedId = ValidateJobTitleId(jobTitleId);
        if (validatedId == JobTitleId) return;
        JobTitleId = validatedId;
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

    // ---- Helpers ----

    private void AddPhoneInternal(EmployeePhone phone)
    {
        if (phone is null) throw new ArgumentNullException(nameof(phone));
        if (_phones.Any(p => p.PhoneNumber.E164 == phone.PhoneNumber.E164))
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