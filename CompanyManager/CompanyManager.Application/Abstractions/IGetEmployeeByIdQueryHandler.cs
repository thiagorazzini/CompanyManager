using CompanyManager.Application.DTOs;

namespace CompanyManager.Application.Abstractions;

public interface IGetEmployeeByIdQueryHandler
{
    Task<GetEmployeeByIdResponse?> Handle(GetEmployeeByIdRequest request, CancellationToken cancellationToken);
}
