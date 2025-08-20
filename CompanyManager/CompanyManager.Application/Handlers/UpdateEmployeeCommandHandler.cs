using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.ValueObjects;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyManager.Application.Handlers
{
    public sealed class UpdateEmployeeCommandHandler : IUpdateEmployeeCommandHandler
    {
        private readonly IEmployeeRepository _employees;
        private readonly IDepartmentRepository _departments;
        private readonly IValidator<UpdateEmployeeRequest> _validator;

        public UpdateEmployeeCommandHandler(
            IEmployeeRepository employees, 
            IDepartmentRepository departments,
            IValidator<UpdateEmployeeRequest> validator)
        {
            _employees = employees;
            _departments = departments;
            _validator = validator;
        }

        public async Task Handle(UpdateEmployeeCommand cmd, CancellationToken ct)
        {
            // 0) Validar o comando antes de processar
            var updateRequest = new UpdateEmployeeRequest
            {
                Id = cmd.Id,
                FirstName = cmd.FirstName,
                LastName = cmd.LastName,
                Email = cmd.Email,
                DocumentNumber = cmd.DocumentNumber,
                PhoneNumbers = cmd.Phones?.ToList() ?? new List<string>(),
                JobTitle = cmd.JobTitle,
                DepartmentId = cmd.DepartmentId
            };

            var validationResult = await _validator.ValidateAsync(updateRequest, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }

            // 1) carregar funcionário
            var employee = await _employees.GetByIdAsync(cmd.Id, ct);
            if (employee is null)
                throw new ArgumentException("Employee not found.", nameof(cmd.Id));

            // 2) verificar existência do departamento
            if (!await _departments.ExistsAsync(cmd.DepartmentId, ct))
                throw new ArgumentException("Department does not exist.", nameof(cmd.DepartmentId));

            // 3) normalizações básicas
            var incomingEmailNorm = (cmd.Email ?? string.Empty).Trim().ToLowerInvariant();
            var currentEmailNorm = employee.Email.Value.Trim().ToLowerInvariant();

            // 4) checar duplicidade de email somente se mudou
            if (!string.Equals(incomingEmailNorm, currentEmailNorm, StringComparison.OrdinalIgnoreCase))
            {
                if (await _employees.EmailExistsAsync(incomingEmailNorm, ct))
                    throw new InvalidOperationException("Email already in use.");
            }

            // 5) checar duplicidade de CPF somente se mudou (usa VO para obter apenas dígitos)
            var incomingDoc = new DocumentNumber(cmd.DocumentNumber ?? string.Empty);
            if (!string.Equals(incomingDoc.Digits, employee.DocumentNumber.Digits, StringComparison.Ordinal))
            {
                if (await _employees.CpfExistsAsync(incomingDoc.Digits, ct))
                    throw new InvalidOperationException("Document number already in use.");
            }

            // 6) aplicar mudanças (VOs validam formatos)
            employee.ChangeName(cmd.FirstName ?? string.Empty, cmd.LastName ?? string.Empty);
            employee.ChangeEmail(new Email((cmd.Email ?? string.Empty).Trim()));
            employee.ChangeDocument(new DocumentNumber((cmd.DocumentNumber ?? string.Empty).Trim()));
            employee.ChangeJobTitle(cmd.JobTitle ?? string.Empty);
            employee.ChangeDepartment(cmd.DepartmentId);

            // Update manager if changed
            if (cmd.ManagerId != employee.ManagerId)
            {
                if (cmd.ManagerId.HasValue)
                {
                    // Validate that the new manager exists
                    if (!await _departments.ExistsAsync(cmd.ManagerId.Value, ct))
                        throw new ArgumentException("Manager does not exist.", nameof(cmd.ManagerId));
                    
                    employee.AssignManager(cmd.ManagerId.Value);
                }
                else
                {
                    employee.RemoveManager();
                }
            }

            // 7) sincronizar telefones (remove ausentes e adiciona novos)
            var requestedPhones = cmd.Phones ?? Array.Empty<string>();

            var targetPhones = requestedPhones
                .Select(p => new PhoneNumber(p, defaultCountry: "BR"))
                .ToHashSet();

            if (targetPhones.Count == 0)
                throw new ArgumentException("At least one phone number is required.", nameof(cmd.Phones));


            foreach (var phone in targetPhones)
            {
                if (!employee.Phones.Contains(phone))
                    employee.AddPhone(phone);
            }

            // remove os que não estão mais presentes
            foreach (var existing in employee.Phones.ToArray())
            {
                if (!targetPhones.Contains(existing))
                    employee.RemovePhone(existing);
            }

            // 8) persistir
            await _employees.UpdateAsync(employee, ct);
        }
    }
}
