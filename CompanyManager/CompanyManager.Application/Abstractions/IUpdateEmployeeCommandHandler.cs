using CompanyManager.Application.Commands;

namespace CompanyManager.Application.Abstractions
{
    public interface IUpdateEmployeeCommandHandler
    {
        Task Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken, Guid? currentUserId = null);
    }
}
