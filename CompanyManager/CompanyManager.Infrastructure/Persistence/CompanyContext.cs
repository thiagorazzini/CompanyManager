using Microsoft.EntityFrameworkCore;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.AccessControl;
using CompanyManager.Domain.ValueObjects;

namespace CompanyManager.Infrastructure.Persistence
{
    public class CompanyContext : DbContext
    {
        public CompanyContext(DbContextOptions<CompanyContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeePhone> EmployeePhones { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar todas as configurações de entidades
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CompanyContext).Assembly);

            // Configuração específica para UserName como Value Object
            modelBuilder.Entity<UserAccount>()
                .Property(u => u.UserName)
                .HasMaxLength(255)
                .IsRequired()
                .HasComment("Nome de usuário (email) normalizado");

            // Configurações de Value Objects serão feitas nas configurações específicas das entidades
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Configurar convenções globais
            configurationBuilder.Properties<DateTime>()
                .HaveColumnType("datetime2");
            
            configurationBuilder.Properties<DateTimeOffset>()
                .HaveColumnType("datetimeoffset");
            
            configurationBuilder.Properties<decimal>()
                .HavePrecision(18, 2);
        }
    }
}
