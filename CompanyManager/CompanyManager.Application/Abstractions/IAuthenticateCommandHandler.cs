using CompanyManager.Application.Auth;
using CompanyManager.Application.Commands;
using CompanyManager.Application.Services;

namespace CompanyManager.Application.Abstractions
{
    public interface IAuthenticateCommandHandler
    {
        Task<AuthResult> Handle(AuthenticateCommand command, CancellationToken cancellationToken);
    }
}
