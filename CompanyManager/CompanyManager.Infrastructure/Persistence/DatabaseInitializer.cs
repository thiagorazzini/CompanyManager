using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;
using CompanyManager.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

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

                // Verificar se já existem dados
                if (await context.Departments.AnyAsync())
                    return;

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

                            // Criar funcionário administrador padrão
            var adminEmployee = Employee.Create(
                "Administrador",
                "Sistema",
                new Email("admin@companymanager.com"),
                new DocumentNumber("12345678901"),
                new DateOfBirth(new DateTime(1990, 1, 1)),
                new[] { new PhoneNumber("+5511999999999") },
                "Administrador do Sistema",
                departments.First(d => d.Name == "Tecnologia da Informação").Id
            );

            await context.Employees.AddAsync(adminEmployee);

            // Criar usuário administrador padrão
            var adminRole = roles.First(r => r.Name == "Director");
            var adminUser = UserAccount.Create("admin@companymanager.com", passwordHasher.Hash("Admin123!"), adminEmployee.Id);
            adminUser.AddRole(adminRole);

            await context.UserAccounts.AddAsync(adminUser);

                // Salvar todas as alterações
                await context.SaveChangesAsync();
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
    }
}
