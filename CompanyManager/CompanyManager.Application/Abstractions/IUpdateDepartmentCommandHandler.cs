using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Abstractions
{
    public interface IUpdateDepartmentCommandHandler
    {
        Task Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken);
    }
}
