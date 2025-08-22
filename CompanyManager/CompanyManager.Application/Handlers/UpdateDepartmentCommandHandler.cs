using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Validators;
using FluentValidation;
using System;

namespace CompanyManager.Application.Handlers
{
    public sealed class UpdateDepartmentCommandHandler : IUpdateDepartmentCommandHandler
    {
        private readonly IDepartmentRepository _departments;
        private readonly IValidator<UpdateDepartmentRequest> _validator;

        public UpdateDepartmentCommandHandler(
            IDepartmentRepository departments,
            IValidator<UpdateDepartmentRequest> validator)
        {
            _departments = departments;
            _validator = validator;
        }

        public async Task Handle(UpdateDepartmentCommand cmd, CancellationToken ct)
        {
            var updateRequest = new UpdateDepartmentRequest
            {
                Id = cmd.Id,
                NewName = cmd.NewName
            };

            var validationResult = await _validator.ValidateAsync(updateRequest, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Validation failed: {errors}");
            }

            var dept = await _departments.GetByIdAsync(cmd.Id, ct);
            if (dept is null)
                throw new ArgumentException("Department not found.", nameof(cmd.Id));

            dept.Rename(cmd.NewName);

            await _departments.UpdateAsync(dept, ct);
        }
    }
}
