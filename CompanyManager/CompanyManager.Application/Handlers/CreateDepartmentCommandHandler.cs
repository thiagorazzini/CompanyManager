using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using CompanyManager.Domain.Entities;
using FluentValidation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CompanyManager.Application.Handlers
{
    /// <summary>
    /// Handles department creation by validating input and persisting new departments
    /// </summary>
    public sealed class CreateDepartmentCommandHandler : ICreateDepartmentCommandHandler
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IValidator<CreateDepartmentRequest> _validator;

        public CreateDepartmentCommandHandler(
            IDepartmentRepository departmentRepository,
            IValidator<CreateDepartmentRequest> validator)
        {
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        /// Handles the department creation command by orchestrating validation and persistence
        /// </summary>
        public async Task<Guid> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
        {
            await ValidateRequestAsync(command, cancellationToken);
            
            var department = CreateDepartment(command);
            await PersistDepartmentAsync(department, cancellationToken);
            
            return department.Id;
        }

        /// <summary>
        /// Validates the create department request using FluentValidation
        /// </summary>
        private async Task ValidateRequestAsync(CreateDepartmentCommand command, CancellationToken cancellationToken)
        {
            var createRequest = new CreateDepartmentRequest
            {
                Name = command.Name
            };

            var validationResult = await _validator.ValidateAsync(createRequest, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }
        }

        /// <summary>
        /// Creates a new department entity with the provided data
        /// </summary>
        private static Department CreateDepartment(CreateDepartmentCommand command)
        {
            return Department.Create(command.Name, command.Description);
        }

        /// <summary>
        /// Persists the department to the repository
        /// </summary>
        private async Task PersistDepartmentAsync(Department department, CancellationToken cancellationToken)
        {
            await _departmentRepository.AddAsync(department, cancellationToken);
        }
    }
}
