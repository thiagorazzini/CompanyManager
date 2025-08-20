using CompanyManager.Domain.Common;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.DTOs;
using System;
using CompanyManager.Application.Abstractions;

namespace CompanyManager.Application.Queries
{
    public sealed class ListEmployeesQueryHandler : IListEmployeesQueryHandler
    {
        private readonly IEmployeeRepository _employees;
        
        public ListEmployeesQueryHandler(IEmployeeRepository employees) => _employees = employees;

        public async Task<PageResult<Employee>> Handle(ListEmployeesRequest request, CancellationToken ct)
        {
            var filter = new EmployeeFilter(
                DepartmentId: request.DepartmentId,
                NameOrEmail: string.IsNullOrWhiteSpace(request.NameContains) ? null : request.NameContains.Trim(),
                JobTitle: string.IsNullOrWhiteSpace(request.JobTitle) ? null : request.JobTitle.Trim()
            );

            var page = new PageRequest(
                Math.Max(request.Page, 1),
                Math.Clamp(request.PageSize, 1, 100)
            );

            var (items, total) = await _employees.SearchAsync(filter, page, ct);
            return new PageResult<Employee>(items, total, page.Page, page.PageSize);
        }
    }
}
