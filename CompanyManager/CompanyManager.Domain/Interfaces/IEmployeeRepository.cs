using CompanyManager.Domain.Common;
using CompanyManager.Domain.Entities;

namespace CompanyManager.Domain.Interfaces
{
    public interface IEmployeeRepository
    {
        // Métodos de leitura
        Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Employee?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetByJobTitleAsync(string jobTitle, CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Employee>> GetInactiveEmployeesAsync(CancellationToken cancellationToken = default);

        // Métodos de busca com filtros e paginação
        Task<(IReadOnlyList<Employee> Items, int Total)> SearchAsync(
            EmployeeFilter filter, PageRequest page, CancellationToken cancellationToken = default);

        // Métodos de verificação de existência
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default);
        Task<bool> CpfExistsAsync(string cpfDigitsOnly, CancellationToken cancellationToken = default);

        // Métodos de CRUD
        Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
        Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        // Métodos de consulta em lote
        Task<IEnumerable<Employee>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> GetCountByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
        Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all distinct job titles available in the system
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of unique job titles</returns>
        Task<IEnumerable<string>> GetDistinctJobTitlesAsync(CancellationToken cancellationToken = default);
    }
}
