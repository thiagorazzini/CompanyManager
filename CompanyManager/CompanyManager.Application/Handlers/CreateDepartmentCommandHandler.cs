using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyManager.Application.Handlers
{
    public sealed class CreateDepartmentCommandHandler : ICreateDepartmentCommandHandler
    {
        private readonly IDepartmentRepository _departments;
        private readonly IValidator<CreateDepartmentRequest> _validator;

        public CreateDepartmentCommandHandler(
            IDepartmentRepository departments,
            IValidator<CreateDepartmentRequest> validator)
        {
            _departments = departments;
            _validator = validator;
        }

        public async Task<Guid> Handle(CreateDepartmentCommand cmd, CancellationToken ct)
        {
            // 0) Validar o comando antes de processar
            var createRequest = new CreateDepartmentRequest
            {
                Name = cmd.Name
            };

            var validationResult = await _validator.ValidateAsync(createRequest, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }

            // 1) Criar o departamento (a entidade também valida internamente)
            var department = Department.Create(cmd.Name);

            // 2) Persistir
            await _departments.AddAsync(department, ct);

            return department.Id;
        }
    }
}
