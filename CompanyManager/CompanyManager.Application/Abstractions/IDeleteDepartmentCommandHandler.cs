using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Abstractions
{
    public interface IDeleteDepartmentCommandHandler
    {
        Task Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken);
    }
}
