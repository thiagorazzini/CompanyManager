using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;

namespace CompanyManager.Infrastructure.Persistence.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedNever();

            // Propriedades básicas
            builder.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Nome do funcionário");

            builder.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Sobrenome do funcionário");

            builder.Property(e => e.JobTitleId)
                .IsRequired()
                .HasComment("ID do cargo do funcionário");

            // Configurar Value Objects usando conversores
            builder.Property(e => e.Email)
                .HasConversion(
                    email => email.Value,
                    value => new Email(value))
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("Email")
                .HasComment("Email do funcionário");

            builder.Property(e => e.DocumentNumber)
                .HasConversion(
                    doc => doc.Raw,
                    value => new DocumentNumber(value))
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("DocumentNumber")
                .HasComment("Número do documento (CPF, RG, etc.)");

            builder.Property(e => e.DateOfBirth)
                .HasConversion(
                    dob => dob.BirthDate,
                    value => new DateOfBirth(value))
                .IsRequired()
                .HasColumnName("DateOfBirth")
                .HasComment("Data de nascimento do funcionário");

            // Relacionamento com EmployeePhones
            builder.HasMany(e => e.Phones)
                .WithOne(p => p.Employee)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(e => e.DepartmentId)
                .HasDatabaseName("IX_Employees_DepartmentId");
            
            builder.HasIndex(e => e.JobTitleId)
                .HasDatabaseName("IX_Employees_JobTitleId");

            // ÍNDICES ÚNICOS CRÍTICOS para evitar duplicatas
            builder.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Employees_Email_Unique");
            
            builder.HasIndex(e => e.DocumentNumber)
                .IsUnique()
                .HasDatabaseName("IX_Employees_DocumentNumber_Unique");

            // Relacionamentos
            builder.HasOne(e => e.JobTitle)
                .WithMany(jt => jt.Employees)
                .HasForeignKey(e => e.JobTitleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurações de auditoria são herdadas de BaseEntity
        }
    }
}
