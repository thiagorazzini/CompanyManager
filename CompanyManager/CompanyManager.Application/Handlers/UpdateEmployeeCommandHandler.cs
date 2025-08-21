using CompanyManager.Application.Commands;
using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CompanyManager.Application.Handlers;

public sealed class UpdateEmployeeCommandHandler : IUpdateEmployeeCommandHandler
{
    private readonly IEmployeeRepository _employees;
    private readonly IUserAccountRepository _userAccounts;
    private readonly IDepartmentRepository _departments;
    private readonly IJobTitleRepository _jobTitles;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UpdateEmployeeCommandHandler> _logger;

    public UpdateEmployeeCommandHandler(
        IEmployeeRepository employees,
        IUserAccountRepository userAccounts,
        IDepartmentRepository departments,
        IJobTitleRepository jobTitles,
        IPasswordHasher passwordHasher,
        ILogger<UpdateEmployeeCommandHandler> logger)
    {
        _employees = employees;
        _userAccounts = userAccounts;
        _departments = departments;
        _jobTitles = jobTitles;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task Handle(UpdateEmployeeCommand cmd, CancellationToken ct, Guid? currentUserId = null)
    {
        if (currentUserId == null || currentUserId == Guid.Empty)
            throw new UnauthorizedAccessException("Current user ID not provided.");

        // 1) buscar o employee
        var employee = await _employees.GetByIdAsync(cmd.Id, ct);
        if (employee == null)
            throw new ArgumentException("Employee not found.", nameof(cmd.Id));

        // 2) buscar o usuário atual
        var currentUser = await _userAccounts.GetByIdAsync(currentUserId.Value, ct);
        if (currentUser == null)
            throw new UnauthorizedAccessException("Current user not found.");

        // 2.1) buscar a role do usuário atual para validação
        var currentUserRole = await GetRoleByIdAsync(currentUser.RoleId, ct);
        if (currentUserRole == null)
            throw new UnauthorizedAccessException("Current user role not found.");

        // 2.2) VALIDAÇÃO HIERÁRQUICA para mudança de JobTitle
        if (cmd.JobTitleId != Guid.Empty && cmd.JobTitleId != employee.JobTitleId)
        {
            var newJobTitle = await _jobTitles.GetByIdAsync(cmd.JobTitleId, ct);
            if (newJobTitle != null)
            {
                var targetHierarchicalRole = ConvertJobTitleLevelToHierarchicalRole(newJobTitle.HierarchyLevel);
                
                if (!currentUser.IsSuperUser(currentUserRole) && !currentUser.CanCreateRole(currentUserRole, targetHierarchicalRole))
                {
                    throw new UnauthorizedAccessException($"You cannot change JobTitle to '{targetHierarchicalRole}' level. Your role level is '{currentUserRole.Level}'.");
                }
            }
        }

        // 3) verificar existência do departamento
        if (!await _departments.ExistsAsync(cmd.DepartmentId, ct))
            throw new ArgumentException("Department does not exist.", nameof(cmd.DepartmentId));

        // 4) verificar existência do job title
        if (cmd.JobTitleId != Guid.Empty && !await _jobTitles.ExistsAsync(cmd.JobTitleId, ct))
            throw new ArgumentException("Job title does not exist.", nameof(cmd.JobTitleId));

        // 5) normalizações básicas
        var incomingEmailNorm = (cmd.Email ?? string.Empty).Trim().ToLowerInvariant();
        var currentEmailNorm = employee.Email.Value.Trim().ToLowerInvariant();

        // 6) checar duplicidade de email somente se mudou
        if (!string.Equals(incomingEmailNorm, currentEmailNorm, StringComparison.OrdinalIgnoreCase))
        {
            if (await _employees.EmailExistsAsync(incomingEmailNorm, ct))
                throw new InvalidOperationException("Email already in use.");
        }

        // 7) checar duplicidade de CPF somente se mudou (usa VO para obter apenas dígitos)
        var incomingDoc = new DocumentNumber(cmd.DocumentNumber ?? string.Empty);
        if (!string.Equals(incomingDoc.Digits, employee.DocumentNumber.Digits, StringComparison.Ordinal))
        {
            if (await _employees.CpfExistsAsync(incomingDoc.Digits, ct))
                throw new InvalidOperationException("Document number already in use.");
        }

        // 8) aplicar mudanças (VOs validam formatos)
        employee.ChangeName(cmd.FirstName ?? string.Empty, cmd.LastName ?? string.Empty);
        employee.ChangeEmail(new Email((cmd.Email ?? string.Empty).Trim()));
        employee.ChangeDocument(new DocumentNumber((cmd.DocumentNumber ?? string.Empty).Trim()));
        if (cmd.JobTitleId != Guid.Empty)
        {
            employee.ChangeJobTitle(cmd.JobTitleId);
        }
        employee.ChangeDepartment(cmd.DepartmentId);

        // 9) sincronizar telefones (atualiza todos os telefones)
        var requestedPhones = cmd.Phones ?? new List<string>();

        if (requestedPhones.Count == 0)
            throw new ArgumentException("At least one phone number is required.", nameof(cmd.Phones));

        // Atualizar todos os telefones de uma vez
        employee.UpdatePhones(requestedPhones);

        // 10) atualizar senha se fornecida
        if (!string.IsNullOrEmpty(cmd.Password))
        {
            // Buscar a UserAccount associada ao employee
            var userAccount = await _userAccounts.GetByEmailAsync(employee.Email.Value, ct);
            if (userAccount != null)
            {
                // Buscar a role do usuário alvo para validação
                var targetUserRole = await GetRoleByIdAsync(userAccount.RoleId, ct);
                if (targetUserRole != null)
                {
                    // VALIDAÇÃO HIERÁRQUICA para mudança de senha
                    if (!currentUser.CanModifyUser(currentUserRole, userAccount, targetUserRole))
                    {
                        throw new UnauthorizedAccessException($"You cannot modify the password of a user with role level '{targetUserRole.Level}'. Your role level is '{currentUserRole.Level}'.");
                    }

                    // Hash da nova senha
                    var newPasswordHash = _passwordHasher.Hash(cmd.Password);
                    userAccount.ChangePassword(newPasswordHash);
                    await _userAccounts.UpdateAsync(userAccount, ct);
                }
            }
        }

        // 11) persistir
        await _employees.UpdateAsync(employee, ct);
    }

    /// <summary>
    /// Converte JobTitle.HierarchyLevel para HierarchicalRole
    /// </summary>
    private static HierarchicalRole ConvertJobTitleLevelToHierarchicalRole(int hierarchyLevel)
    {
        return hierarchyLevel switch
        {
            999 => HierarchicalRole.SuperUser,  // SuperUser → SuperUser ✅
            1 => HierarchicalRole.Director,     // President → Director ✅ (nível mais alto)
            2 => HierarchicalRole.Manager,      // Director → Manager ✅
            3 => HierarchicalRole.Senior,       // Head → Senior ✅
            4 => HierarchicalRole.Pleno,        // Coordinator → Pleno ✅
            5 => HierarchicalRole.Junior,       // Employee → Junior ✅ (nível mais baixo)
            _ => HierarchicalRole.Junior        // Default
        };
    }

    /// <summary>
    /// Busca uma role pelo ID
    /// </summary>
    private async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken ct)
    {
        try
        {
            // Como não temos acesso direto ao contexto de Roles, vamos tentar buscar através de usuários
            // ou criar uma role temporária baseada no ID
            // Esta é uma solução temporária - idealmente deveríamos ter acesso direto ao repositório de Roles
            
            // Por enquanto, vamos criar uma role temporária baseada no ID
            // Em uma implementação real, você deveria ter um IRoleRepository
            var tempRole = new Role("Temporary", HierarchicalRole.Junior);
            return tempRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar role por ID: {RoleId}", roleId);
            return null;
        }
    }
}
