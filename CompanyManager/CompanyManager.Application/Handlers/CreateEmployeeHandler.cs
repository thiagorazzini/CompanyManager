using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using CompanyManager.Domain.AccessControl;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace CompanyManager.Application.Handlers;

public sealed class CreateEmployeeHandler
{
    private readonly CreateEmployeeRequestValidator _validator;
    private readonly IEmployeeRepository _employees;
    private readonly IUserAccountRepository _users;
    private readonly IDepartmentRepository _departments;
    private readonly IJobTitleRepository _jobTitles;
    private readonly IRoleRepository _roles;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<CreateEmployeeHandler> _logger;

    public CreateEmployeeHandler(
        CreateEmployeeRequestValidator validator,
        IEmployeeRepository employees,
        IUserAccountRepository users,
        IDepartmentRepository departments,
        IJobTitleRepository jobTitles,
        IRoleRepository roles,
        IPasswordHasher hasher,
        ILogger<CreateEmployeeHandler> logger)
    {
        _validator = validator;
        _employees = employees;
        _users = users;
        _departments = departments;
        _jobTitles = jobTitles;
        _roles = roles;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<CreateEmployeeResult> Handle(CreateEmployeeCommand command, CancellationToken ct, Guid? currentUserId = null)
    {
        try
        {
            _logger.LogInformation("Starting employee creation. Email: {Email}", command.Email);

            var request = new CreateEmployeeRequest
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                DocumentNumber = command.DocumentNumber,
                DateOfBirth = command.DateOfBirth,
                PhoneNumbers = command.Phones,
                JobTitleId = command.JobTitleId,
                DepartmentId = command.DepartmentId,
                Password = command.Password
            };

            var validation = _validator.Validate(request);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Validation failed for employee {Email}. Errors: {Errors}", 
                    command.Email, string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));
                throw new ValidationException(validation.Errors);
            }

            var userId = currentUserId ?? Guid.Empty;
            
            // Validar se o usuário atual foi fornecido
            if (userId == Guid.Empty)
            {
                _logger.LogError("Current user ID not provided");
                throw new UnauthorizedAccessException("Current user ID not provided.");
            }

            // Buscar o JobTitle para obter o nível hierárquico
            var jobTitle = await _jobTitles.GetByIdAsync(command.JobTitleId, ct);
            if (jobTitle == null)
            {
                _logger.LogWarning("Job title not found. JobTitleId: {JobTitleId}", command.JobTitleId);
                throw new ArgumentException("Job title not found.", nameof(command.JobTitleId));
            }

            var currentUser = await _users.GetByIdAsync(userId, ct);
            if (currentUser == null)
            {
                _logger.LogError("Current user not found. UserId: {CurrentUserId}", userId);
                throw new UnauthorizedAccessException("Current user not found.");
            }

            // Buscar a role do usuário atual para validação
            var currentUserRole = await GetRoleByIdAsync(currentUser.RoleId, ct);
            if (currentUserRole == null)
            {
                _logger.LogError("Current user role not found. RoleId: {RoleId}", currentUser.RoleId);
                throw new UnauthorizedAccessException("Current user role not found.");
            }

            // DEBUG: Verificar a role do usuário atual
            _logger.LogInformation("DEBUG: Current user found - ID: {UserId}, UserName: {UserName}, Role: {RoleName}(Level:{RoleLevel})", 
                currentUser.Id, currentUser.UserName, currentUserRole.Name, currentUserRole.Level);

            _logger.LogInformation("DEBUG: JobTitle encontrado - Nome: '{JobTitleName}', ID: {JobTitleId}, HierarchyLevel: {HierarchyLevel}", 
                jobTitle.Name, jobTitle.Id, jobTitle.HierarchyLevel);

            // Converter o nível hierárquico do JobTitle para HierarchicalRole
            var targetRoleLevel = ConvertJobTitleLevelToHierarchicalRole(jobTitle.HierarchyLevel);
            
            _logger.LogInformation("DEBUG: Target role level: {TargetRoleLevel}", targetRoleLevel);

            // DEBUG: Log detalhado da validação hierárquica
            _logger.LogInformation("DEBUG: Validando permissão hierárquica - Usuário: {CurrentUserRoleLevel}, Alvo: {TargetRoleLevel}", 
                currentUserRole.Level, targetRoleLevel);
            
            var canCreate = currentUser.CanCreateRole(currentUserRole, targetRoleLevel);
            _logger.LogInformation("DEBUG: Resultado da validação: {CanCreate}", canCreate);
            
            // Validar se o usuário atual pode criar funcionários com o role especificado
            if (!canCreate)
            {
                _logger.LogWarning("User {CurrentUserId} cannot create employees with role level '{TargetRoleLevel}'. Current user role level: '{CurrentUserRoleLevel}'", 
                    currentUser.Id, targetRoleLevel, currentUserRole.Level);
                throw new UnauthorizedAccessException($"You cannot create employees with role level '{targetRoleLevel}'. Your role level is '{currentUserRole.Level}'.");
            }

            _logger.LogInformation("Hierarchical validation passed. User {CurrentUserId} can create employees with role level '{TargetRoleLevel}'", 
                currentUser.Id, targetRoleLevel);

            // Normalizar o email para o nome de usuário
            var normalizedEmail = command.Email.Trim().ToLowerInvariant();

            // Validar se o email já está em uso
            var existingUser = await _users.GetByEmailAsync(normalizedEmail, ct);
            if (existingUser != null)
            {
                _logger.LogWarning("Email already in use: {Email}", normalizedEmail);
                throw new ArgumentException($"Email '{command.Email}' is already in use.");
            }

            // Validar se o departamento existe
            var department = await _departments.GetByIdAsync(command.DepartmentId, ct);
            if (department == null)
            {
                _logger.LogWarning("Department not found. DepartmentId: {DepartmentId}", command.DepartmentId);
                throw new ArgumentException("Department not found.", nameof(command.DepartmentId));
            }

            // Criar o funcionário
            var document = new DocumentNumber(command.DocumentNumber.Trim());
            var dobDate = DateTime.ParseExact(
                    command.DateOfBirth.Trim(),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);

            var dateOfBirth = new DateOfBirth(dobDate);
            var email = new Email(command.Email.Trim());
            var employee = Employee.Create(
                command.FirstName,
                command.LastName,
                email,
                document,
                dateOfBirth,
                command.Phones,
                command.JobTitleId,
                command.DepartmentId);

            await _employees.AddAsync(employee, ct);

            // Mapear o JobTitle para a role correta baseada no nível hierárquico
            var roleName = GetRoleNameByHierarchyLevel(jobTitle.HierarchyLevel);
            
            _logger.LogInformation("Buscando role existente: {RoleName}", roleName);
            
            // Buscar a role diretamente no contexto
            var existingRole = await GetRoleDirectlyAsync(roleName, ct);
            
            if (existingRole == null)
            {
                _logger.LogError("Falha ao obter role '{RoleName}' para o funcionário", roleName);
                throw new InvalidOperationException($"Não foi possível obter a role '{roleName}' para criar o usuário");
            }
            
            // Validar se a role tem ID válido
            if (existingRole.Id == Guid.Empty)
            {
                _logger.LogError("Role '{RoleName}' retornada com ID inválido", roleName);
                throw new InvalidOperationException($"Role '{roleName}' tem ID inválido");
            }
            
            _logger.LogInformation("Role '{RoleName}' obtida com sucesso. ID: {RoleId}", roleName, existingRole.Id);
            
            // DEBUG: Log detalhado da criação do UserAccount
            _logger.LogInformation("DEBUG: Criando UserAccount - UserName: {UserName}, EmployeeId: {EmployeeId}, RoleId: {RoleId}, JobTitleId: {JobTitleId}", 
                normalizedEmail, employee.Id, existingRole.Id, command.JobTitleId);
            
            // Criar o usuário com a role validada
            var account = UserAccount.Create(
                userName: normalizedEmail,
                passwordHash: _hasher.Hash(command.Password),
                employeeId: employee.Id,
                roleId: existingRole.Id,
                jobTitleId: command.JobTitleId);
            
            _logger.LogInformation("DEBUG: UserAccount criado com sucesso - ID: {AccountId}", account.Id);
                
            await _users.AddAsync(account, ct);
            
            _logger.LogInformation("DEBUG: UserAccount adicionado ao repositório com sucesso");

            _logger.LogInformation("Employee and user account created successfully. EmployeeId: {EmployeeId}, Email: {Email}, Role: {Role}", 
                employee.Id, normalizedEmail, roleName);

            return new CreateEmployeeResult { Id = employee.Id };
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Validation error in value objects: {Message}", ex.Message);
            throw new ValidationException($"Validation error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating employee: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Converte JobTitle.HierarchyLevel para HierarchicalRole
    /// </summary>
    private static HierarchicalRole ConvertJobTitleLevelToHierarchicalRole(int hierarchyLevel)
    {
        var result = hierarchyLevel switch
        {
            999 => HierarchicalRole.SuperUser,  // SuperUser → SuperUser ✅
            1 => HierarchicalRole.Director,     // President → Director ✅ (nível mais alto)
            2 => HierarchicalRole.Manager,      // Director → Manager ✅
            3 => HierarchicalRole.Senior,       // Head → Senior ✅
            4 => HierarchicalRole.Pleno,        // Coordinator → Pleno ✅
            5 => HierarchicalRole.Junior,       // Employee → Junior ✅ (nível mais baixo)
            _ => HierarchicalRole.Junior        // Default
        };
        
        // Log para debug
        System.Diagnostics.Debug.WriteLine($"ConvertJobTitleLevelToHierarchicalRole: {hierarchyLevel} → {result}");
        
        return result;
    }

    /// <summary>
    /// Mapeia o nível hierárquico para o nome da role
    /// </summary>
    private static string GetRoleNameByHierarchyLevel(int hierarchyLevel)
    {
        return hierarchyLevel switch
        {
            999 => "SuperUser",     // SuperUser
            1 => "Director",        // President → Director (nível mais alto)
            2 => "Manager",         // Director → Manager
            3 => "Senior",          // Head → Senior
            4 => "Pleno",           // Coordinator → Pleno
            5 => "Junior",          // Employee → Junior (nível mais baixo)
            _ => "Junior"           // Default
        };
    }

    /// <summary>
    /// Busca uma role diretamente no contexto usando injeção de dependência
    /// </summary>
    private async Task<Role?> GetRoleDirectlyAsync(string roleName, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Buscando role '{RoleName}' diretamente na tabela Roles", roleName);
            
            // Buscar a role diretamente pelo nome
            var existingRole = await _roles.GetByNameAsync(roleName, ct);
            
            if (existingRole != null)
            {
                _logger.LogInformation("Role '{RoleName}' encontrada diretamente (ID: {RoleId}, Nível: {Level})", 
                    roleName, existingRole.Id, existingRole.Level);
                return existingRole;
            }
            
            _logger.LogWarning("Role '{RoleName}' não encontrada. Criando role temporária para este usuário.", roleName);
            
            // Se não encontrou, criar uma role temporária
            var hierarchicalRole = roleName switch
            {
                "SuperUser" => HierarchicalRole.SuperUser,
                "Director" => HierarchicalRole.Director,
                "Manager" => HierarchicalRole.Manager,
                "Senior" => HierarchicalRole.Senior,
                "Pleno" => HierarchicalRole.Pleno,
                "Junior" => HierarchicalRole.Junior,
                _ => HierarchicalRole.Junior
            };
            
            var tempRole = new Role(roleName, hierarchicalRole);
            _logger.LogInformation("Role temporária '{RoleName}' criada com nível {Level}", roleName, hierarchicalRole);
            
            // IMPORTANTE: Salvar a role temporária no banco para garantir que tenha um ID válido
            _logger.LogInformation("Salvando role temporária '{RoleName}' no banco de dados", roleName);
            var savedRole = await _roles.AddAsync(tempRole, ct);
            
            if (savedRole == null || savedRole.Id == Guid.Empty)
            {
                _logger.LogError("Falha ao salvar role temporária '{RoleName}' no banco", roleName);
                throw new InvalidOperationException($"Não foi possível criar a role temporária '{roleName}'");
            }
            
            _logger.LogInformation("Role temporária '{RoleName}' salva com sucesso (ID: {RoleId})", roleName, savedRole.Id);
            
            return savedRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar role '{RoleName}'", roleName);
            return null;
        }
    }

    /// <summary>
    /// Busca uma role pelo ID
    /// </summary>
    private async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken ct)
    {
        try
        {
            _logger.LogDebug("Buscando role por ID: {RoleId}", roleId);
            var role = await _roles.GetByIdAsync(roleId, ct);
            
            if (role == null)
                _logger.LogWarning("Role não encontrada com ID: {RoleId}", roleId);
            else
                _logger.LogDebug("Role encontrada: {RoleName} (Nível: {Level})", role.Name, role.Level);
            
            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar role por ID: {RoleId}", roleId);
            return null;
        }
    }
}

public class CreateEmployeeResult
{
    public Guid Id { get; set; }
}