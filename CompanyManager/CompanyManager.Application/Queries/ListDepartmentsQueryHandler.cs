using System;
using System.Threading;
using System.Threading.Tasks;
using CompanyManager.Application.Abstractions;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Application.DTOs;
using CompanyManager.Domain.Common;
using CompanyManager.Domain.Entities;
using System.Linq;

namespace CompanyManager.Application.Queries
{
    public sealed class ListDepartmentsQueryHandler : IListDepartmentsQueryHandler
    {
        private readonly IDepartmentRepository _departments;

        public ListDepartmentsQueryHandler(IDepartmentRepository departments)
        {
            _departments = departments;
        }

        public async Task<PageResult<Department>> Handle(ListDepartmentsRequest request, CancellationToken ct)
        {
            var filter = new DepartmentFilter(
                string.IsNullOrWhiteSpace(request.NameContains) ? null : request.NameContains.Trim()
            );

            var page = new PageRequest(
                Math.Max(request.Page, 1),
                Math.Clamp(request.PageSize, 1, 100)
            );

            var (items, total) = await _departments.SearchAsync(filter, page, ct);
            return new PageResult<Department>(items, total, page.Page, page.PageSize);
        }
    }
}
