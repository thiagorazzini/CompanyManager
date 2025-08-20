using CompanyManager.Application.Auth;
using CompanyManager.Application.Commands;
using CompanyManager.Application.Services;

namespace CompanyManager.Application.Abstractions
{
    public interface IRefreshTokenCommandHandler
    {
        Task<AuthResult> Handle(RefreshTokenCommand command, CancellationToken cancellationToken);
    }
}
