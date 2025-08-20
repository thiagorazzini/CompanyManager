using CompanyManager.Application.DTOs;
using CompanyManager.Domain.Common;
using CompanyManager.Domain.Entities;

namespace CompanyManager.Application.Abstractions
{
    public interface IListEmployeesQueryHandler
    {
        Task<PageResult<Employee>> Handle(ListEmployeesRequest request, CancellationToken cancellationToken);
    }
}
