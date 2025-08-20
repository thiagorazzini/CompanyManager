using CompanyManager.Domain.Entities;

namespace CompanyManager.Application.Abstractions
{
    public interface IGetDepartmentByIdQueryHandler
    {
        Task<Department?> Handle(Guid id, CancellationToken cancellationToken);
    }
}
