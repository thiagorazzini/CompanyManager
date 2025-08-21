using CompanyManager.Domain.Common;
using CompanyManager.Domain.ValueObjects;

namespace CompanyManager.Domain.Entities;

/// <summary>
/// Representa um telefone de um funcionário
/// </summary>
public sealed class EmployeePhone : BaseEntity
{
    /// <summary>
    /// ID do funcionário proprietário do telefone
    /// </summary>
    public Guid EmployeeId { get; private set; }
    
    /// <summary>
    /// Número do telefone como Value Object
    /// </summary>
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    
    /// <summary>
    /// Tipo do telefone (Mobile, Work, Home, etc.)
    /// </summary>
    public string Type { get; private set; } = "Mobile";
    
    /// <summary>
    /// Indica se é o telefone principal
    /// </summary>
    public bool IsPrimary { get; private set; }
    
    /// <summary>
    /// Relacionamento com Employee
    /// </summary>
    public Employee? Employee { get; private set; }

    // Construtor para EF Core
    private EmployeePhone() { }

    /// <summary>
    /// Construtor privado para criação controlada
    /// </summary>
    private EmployeePhone(
        Guid employeeId,
        PhoneNumber phoneNumber,
        string type = "Mobile",
        bool isPrimary = false)
    {
        EmployeeId = ValidateEmployeeId(employeeId);
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Type = ValidateType(type);
        IsPrimary = isPrimary;
    }

    /// <summary>
    /// Cria um novo telefone para funcionário
    /// </summary>
    public static EmployeePhone Create(
        Guid employeeId,
        PhoneNumber phoneNumber,
        string type = "Mobile",
        bool isPrimary = false)
        => new(employeeId, phoneNumber, type, isPrimary);

    /// <summary>
    /// Altera o tipo do telefone
    /// </summary>
    public void ChangeType(string type)
    {
        var validatedType = ValidateType(type);
        if (validatedType == Type) return;
        
        Type = validatedType;
        UpdateModifiedAt();
    }

    /// <summary>
    /// Define se é o telefone principal
    /// </summary>
    public void SetAsPrimary(bool isPrimary)
    {
        if (IsPrimary == isPrimary) return;
        
        IsPrimary = isPrimary;
        UpdateModifiedAt();
    }

    /// <summary>
    /// Altera o número do telefone
    /// </summary>
    public void ChangePhoneNumber(PhoneNumber phoneNumber)
    {
        if (phoneNumber is null) throw new ArgumentNullException(nameof(phoneNumber));
        if (PhoneNumber.Equals(phoneNumber)) return;
        
        PhoneNumber = phoneNumber;
        UpdateModifiedAt();
    }

    // Validações
    private static Guid ValidateEmployeeId(Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty.", nameof(employeeId));
        return employeeId;
    }

    private static string ValidateType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Phone type cannot be null or empty.", nameof(type));

        var trimmed = type.Trim();
        var validTypes = new[] { "Mobile", "Work", "Home", "Other" };
        
        if (!validTypes.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Invalid phone type. Valid types: {string.Join(", ", validTypes)}", nameof(type));
        
        return trimmed;
    }
}
