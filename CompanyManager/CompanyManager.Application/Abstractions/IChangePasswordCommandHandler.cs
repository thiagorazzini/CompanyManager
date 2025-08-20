using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Abstractions
{
    public interface IChangePasswordCommandHandler
    {
        Task Handle(ChangePasswordCommand command, CancellationToken cancellationToken);
    }
}
