using CompanyManager.Domain.Interfaces;
using CompanyManager.Domain.Entities;

namespace CompanyManager.Application.Queries
{
    public sealed class GetEmployeeByIdQueryHandler
    {
        private readonly IEmployeeRepository _employees;

        public GetEmployeeByIdQueryHandler(IEmployeeRepository employees)
        {
            _employees = employees;
        }

        public Task<Employee?> Handle(Guid id, CancellationToken ct) =>
            _employees.GetByIdAsync(id, ct);
    }
}
