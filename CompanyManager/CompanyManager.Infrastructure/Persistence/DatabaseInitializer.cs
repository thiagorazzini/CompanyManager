using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using CompanyManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CompanyManager.Infrastructure.Persistence
{
    /// <summary>
    /// Inicializador do banco de dados com dados de seed
    /// </summary>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Inicializa o banco de dados com dados padrão
        /// </summary>
        public static async Task InitializeAsync(CompanyContext context, IPasswordHasher passwordHasher)
        {
            try
            {
                // Garantir que o banco seja criado
                await context.Database.EnsureCreatedAsync();

                // Verificar se o SuperUser existe
                var superUserExists = await context.JobTitles.AnyAsync(jt => jt.HierarchyLevel == 999);
                
                if (!superUserExists)
                {
                    // Criar apenas o SuperUser se não existir
                    var superUserJobTitle = JobTitle.Create("SuperUser", 999, "System administrator with full access");
                    await context.JobTitles.AddAsync(superUserJobTitle);
                    
                    // Criar role SuperUser se não existir
                    var superUserRoleExists = await context.Roles.AnyAsync(r => r.Name == "SuperUser");
                    if (!superUserRoleExists)
                    {
                        var superUserRole = new Role("SuperUser", HierarchicalRole.SuperUser);
                        await context.Roles.AddAsync(superUserRole);
                    }
                    
                    await context.SaveChangesAsync();
                }

                // Verificar se já existem outros dados
                if (await context.JobTitles.CountAsync() > 1)
                {
                    // Atualizar o administrador existente para usar o JobTitle SuperUser
                    await UpdateAdminToSuperUserAsync(context);
                    return;
                }

                // Criar job titles padrão com níveis hierárquicos CORRETOS (apenas se não existirem)
                var jobTitles = new[]
                {
                    JobTitle.Create("President", 1, "Top level executive, company president"),
                    JobTitle.Create("Director", 2, "Senior management, department director"),
                    JobTitle.Create("Head", 3, "Department head, team leader"),
                    JobTitle.Create("Coordinator", 4, "Project coordinator, team coordinator"),
                    JobTitle.Create("Employee", 5, "Regular employee, individual contributor")
                };

                await context.JobTitles.AddRangeAsync(jobTitles);

                // Criar departamentos padrão
                var departments = new[]
                {
                    Department.Create("Recursos Humanos", "Departamento responsável pela gestão de pessoas"),
                    Department.Create("Tecnologia da Informação", "Departamento responsável pela infraestrutura de TI"),
                    Department.Create("Financeiro", "Departamento responsável pelas finanças da empresa")
                };

                await context.Departments.AddRangeAsync(departments);

                // Criar roles padrão com níveis hierárquicos
                var roles = new[]
                {
                    new Role("Director", HierarchicalRole.Director),
                    new Role("Manager", HierarchicalRole.Manager),
                    new Role("Senior", HierarchicalRole.Senior),
                    new Role("Pleno", HierarchicalRole.Pleno),
                    new Role("Junior", HierarchicalRole.Junior)
                };

                await context.Roles.AddRangeAsync(roles);

                // Criar funcionário administrador padrão (apenas se não existir)
                var allEmployees = await context.Employees.ToListAsync();
                var adminExists = allEmployees.Any(e => e.Email.Value == "admin@companymanager.com");
                if (!adminExists)
                {
                    var adminJobTitle = await context.JobTitles.FirstAsync(jt => jt.HierarchyLevel == 999);
                    var adminEmployee = Employee.Create(
                        "Administrador",
                        "Sistema",
                        new Email("admin@companymanager.com"),
                        new DocumentNumber("11144477735"), // CPF válido
                        new DateOfBirth(new DateTime(1990, 1, 1)),
                        new[] { "+5511999999999" },
                        adminJobTitle.Id,
                        departments.First(d => d.Name == "Tecnologia da Informação").Id
                    );

                    await context.Employees.AddAsync(adminEmployee);

                    // Criar usuário administrador padrão com role SuperUser
                    var adminRole = await context.Roles.FirstAsync(r => r.Name == "SuperUser");
                    var adminUser = UserAccount.Create("admin@companymanager.com", passwordHasher.Hash("Admin123!"), adminEmployee.Id, adminRole.Id, adminJobTitle.Id);

                    await context.UserAccounts.AddAsync(adminUser);
                }

                // Salvar todas as alterações
                await context.SaveChangesAsync();
                
                // Atualizar o administrador existente para usar o JobTitle SuperUser
                await UpdateAdminToSuperUserAsync(context);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao inicializar banco de dados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Limpa todos os dados do banco (útil para testes)
        /// </summary>
        public static async Task ClearAsync(CompanyContext context)
        {
            try
            {
                context.Employees.RemoveRange(context.Employees);
                context.UserAccounts.RemoveRange(context.UserAccounts);
                context.Departments.RemoveRange(context.Departments);
                context.Roles.RemoveRange(context.Roles);

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao limpar banco de dados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Atualiza o usuário administrador existente para usar o JobTitle SuperUser
        /// </summary>
        public static async Task UpdateAdminToSuperUserAsync(CompanyContext context)
        {
            try
            {
                // Buscar o usuário administrador usando client-side evaluation para evitar problemas LINQ
                var allEmployees = await context.Employees.ToListAsync();
                var adminEmployee = allEmployees.FirstOrDefault(e => e.Email.Value == "admin@companymanager.com");
                
                if (adminEmployee != null)
                {
                    // Buscar o JobTitle SuperUser
                    var superUserJobTitle = await context.JobTitles
                        .FirstOrDefaultAsync(jt => jt.HierarchyLevel == 999);
                    
                    if (superUserJobTitle != null)
                    {
                        // Atualizar o JobTitle do administrador apenas se for diferente
                        if (adminEmployee.JobTitleId != superUserJobTitle.Id)
                        {
                            adminEmployee.ChangeJobTitle(superUserJobTitle.Id);
                        }
                        
                        // Buscar a conta de usuário
                        var adminUser = await context.UserAccounts
                            .FirstOrDefaultAsync(u => u.EmployeeId == adminEmployee.Id);
                        
                        if (adminUser != null)
                        {
                            // Buscar a role SuperUser
                            var superUserRole = await context.Roles
                                .FirstOrDefaultAsync(r => r.Name == "SuperUser");
                            
                            if (superUserRole != null)
                            {
                                // Atualizar o RoleId para SuperUser
                                adminUser.SetRole(superUserRole.Id);
                            }
                        }
                        
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao atualizar administrador para SuperUser: {ex.Message}", ex);
            }
        }
    }
}
