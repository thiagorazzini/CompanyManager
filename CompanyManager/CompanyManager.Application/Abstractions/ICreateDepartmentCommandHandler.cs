using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Abstractions
{
    public interface ICreateDepartmentCommandHandler
    {
        Task<Guid> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken);
    }
}
