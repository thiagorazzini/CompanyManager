using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Abstractions
{
    public interface ILogoutCommandHandler
    {
        Task Handle(LogoutCommand command, CancellationToken cancellationToken);
    }
}
