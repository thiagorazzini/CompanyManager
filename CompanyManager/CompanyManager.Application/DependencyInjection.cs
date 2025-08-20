using CompanyManager.Application.Abstractions;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Handlers;
using CompanyManager.Application.Interfaces;
using CompanyManager.Application.Queries;
using CompanyManager.Application.Services;
using CompanyManager.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CompanyManager.Application
{
    /// <summary>
    /// Extension methods for registering Application layer services in the DI container.
    /// </summary>
    /// <remarks>
    /// This class provides a centralized way to register all Application layer services,
    /// including handlers, validators, and services with their respective interfaces.
    /// </remarks>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds Application layer services to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for method chaining.</returns>
        /// <remarks>
        /// Registers the following services:
        /// - Handlers: CreateEmployeeHandler
        /// - Validators: CreateEmployeeRequestValidator
        /// - Services: PasswordHasher, AuthService
        /// - FluentValidation: Validators for DTOs
        /// </remarks>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register handlers
            services.AddScoped<CreateEmployeeHandler>();
            services.AddScoped<IUpdateEmployeeCommandHandler, UpdateEmployeeCommandHandler>();
            services.AddScoped<ICreateDepartmentCommandHandler, CreateDepartmentCommandHandler>();
            services.AddScoped<IUpdateDepartmentCommandHandler, UpdateDepartmentCommandHandler>();
            services.AddScoped<IDeleteDepartmentCommandHandler, DeleteDepartmentCommandHandler>();
            services.AddScoped<IAuthenticateCommandHandler, AuthenticateCommandHandler>();
            services.AddScoped<IRefreshTokenCommandHandler, RefreshTokenCommandHandler>();
            services.AddScoped<IChangePasswordCommandHandler, ChangePasswordCommandHandler>();

            // Register query handlers
            services.AddScoped<IGetDepartmentByIdQueryHandler, GetDepartmentByIdQueryHandler>();
            services.AddScoped<IListDepartmentsQueryHandler, ListDepartmentsQueryHandler>();
            services.AddScoped<IListEmployeesQueryHandler, ListEmployeesQueryHandler>();

            // Register validators
            services.AddScoped<CreateEmployeeRequestValidator>();
            services.AddScoped<IValidator<CreateEmployeeRequest>, CreateEmployeeRequestValidator>();
            services.AddScoped<IValidator<UpdateEmployeeRequest>, UpdateEmployeeRequestValidator>();
            services.AddScoped<IValidator<CreateDepartmentRequest>, CreateDepartmentRequestValidator>();
            services.AddScoped<IValidator<UpdateDepartmentRequest>, UpdateDepartmentRequestValidator>();
            services.AddScoped<IValidator<AuthenticateRequest>, AuthenticateRequestValidator>();
            services.AddScoped<IValidator<RefreshTokenRequest>, RefreshTokenRequestValidator>();
            services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();

            // Register services
            services.AddScoped<PasswordHasher>();
            services.AddScoped<CompanyManager.Domain.Interfaces.IPasswordHasher, PasswordHasher>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
