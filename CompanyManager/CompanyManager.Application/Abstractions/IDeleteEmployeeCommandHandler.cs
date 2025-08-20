using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Abstractions
{
    public interface IDeleteEmployeeCommandHandler
    {
        Task Handle(DeleteEmployeeCommand command, CancellationToken cancellationToken);
    }
}
