using CompanyManager.Application.DTOs;
using CompanyManager.Domain.Common;
using CompanyManager.Domain.Entities;

namespace CompanyManager.Application.Abstractions
{
    public interface IListDepartmentsQueryHandler
    {
        Task<PageResult<Department>> Handle(ListDepartmentsRequest request, CancellationToken cancellationToken);
    }
}
