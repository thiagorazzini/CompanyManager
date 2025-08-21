using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Interfaces;
using CompanyManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CompanyManager.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly CompanyContext _context;
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(CompanyContext context, ILogger<RoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Buscando role por ID: {RoleId}", id);
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
                
                if (role == null)
                    _logger.LogWarning("Role não encontrada com ID: {RoleId}", id);
                else
                    _logger.LogDebug("Role encontrada: {RoleName} (ID: {RoleId})", role.Name, id);
                
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar role por ID: {RoleId}", id);
                throw;
            }
        }

        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Nome da role não pode ser vazio", nameof(name));

                _logger.LogDebug("Buscando role por nome: {RoleName}", name);
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
                
                if (role == null)
                    _logger.LogWarning("Role não encontrada com nome: {RoleName}", name);
                else
                    _logger.LogDebug("Role encontrada: {RoleName} (ID: {RoleId})", role.Name, role.Id);
                
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar role por nome: {RoleName}", name);
                throw;
            }
        }

        public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Buscando todas as roles");
                var roles = await _context.Roles
                    .OrderBy(r => r.Level)
                    .ThenBy(r => r.Name)
                    .ToListAsync(cancellationToken);
                
                _logger.LogDebug("Encontradas {Count} roles", roles.Count);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todas as roles");
                throw;
            }
        }

        public async Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default)
        {
            try
            {
                if (role == null)
                    throw new ArgumentNullException(nameof(role));

                _logger.LogDebug("Adicionando nova role: {RoleName} (Nível: {Level})", role.Name, role.Level);
                
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == role.Name, cancellationToken);
                
                if (existingRole != null)
                {
                    _logger.LogWarning("Role com nome '{RoleName}' já existe (ID: {ExistingRoleId})", 
                        role.Name, existingRole.Id);
                    return existingRole;
                }

                await _context.Roles.AddAsync(role, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Role '{RoleName}' adicionada com sucesso (ID: {RoleId})", 
                    role.Name, role.Id);
                
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar role: {RoleName}", role?.Name);
                throw;
            }
        }

        public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
        {
            try
            {
                if (role == null)
                    throw new ArgumentNullException(nameof(role));

                _logger.LogDebug("Atualizando role: {RoleName} (ID: {RoleId})", role.Name, role.Id);
                
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == role.Id, cancellationToken);
                
                if (existingRole == null)
                {
                    _logger.LogWarning("Role não encontrada para atualização (ID: {RoleId})", role.Id);
                    throw new InvalidOperationException($"Role com ID {role.Id} não encontrada");
                }

                _context.Entry(existingRole).CurrentValues.SetValues(role);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Role '{RoleName}' atualizada com sucesso (ID: {RoleId})", 
                    role.Name, role.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar role: {RoleName} (ID: {RoleId})", 
                    role?.Name, role?.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Removendo role com ID: {RoleId}", id);
                
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
                
                if (role == null)
                {
                    _logger.LogWarning("Role não encontrada para remoção (ID: {RoleId})", id);
                    return;
                }

                // Verificar se há usuários usando esta role
                var usersWithRole = await _context.UserAccounts
                    .AnyAsync(u => u.RoleId == id, cancellationToken);
                
                if (usersWithRole)
                {
                    _logger.LogWarning("Não é possível remover role '{RoleName}' (ID: {RoleId}) - há usuários associados", 
                        role.Name, id);
                    throw new InvalidOperationException($"Role '{role.Name}' não pode ser removida pois há usuários associados");
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Role '{RoleName}' removida com sucesso (ID: {RoleId})", 
                    role.Name, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover role com ID: {RoleId}", id);
                throw;
            }
        }
    }
}
