using CompanyManager.Domain.Common;
using CompanyManager.Domain.Entities;

namespace CompanyManager.Domain.Interfaces
{
    public interface IDepartmentRepository
    {
        // Métodos de leitura
        Task<Department?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<Department?> GetByDescriptionAsync(string description, CancellationToken cancellationToken = default);
        Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Department>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Department>> GetInactiveAsync(CancellationToken cancellationToken = default);

        // Métodos de busca com filtros e paginação
        Task<(IReadOnlyList<Department> Items, int Total)> SearchAsync(
            DepartmentFilter filter, PageRequest page, CancellationToken cancellationToken = default);

        // Métodos de verificação de existência
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> ExistsByDescriptionAsync(string description, CancellationToken cancellationToken = default);

        // Métodos de CRUD
        Task AddAsync(Department department, CancellationToken cancellationToken = default);
        Task UpdateAsync(Department department, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        // Métodos de consulta em lote
        Task<IEnumerable<Department>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> GetEmployeeCountAsync(Guid departmentId, CancellationToken cancellationToken = default);
        Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Department>> GetDepartmentsWithEmployeeCountAsync(CancellationToken cancellationToken = default);
    }
}
