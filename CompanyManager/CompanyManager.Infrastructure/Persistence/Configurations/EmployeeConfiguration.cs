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

            builder.Property(e => e.JobTitle)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Cargo do funcionário");

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

            // Configurar PhoneNumbers como uma propriedade JSON ou tabela separada
            // Por enquanto, vamos ignorar para simplificar
            builder.Ignore(e => e.Phones);

            // Índices
            builder.HasIndex(e => e.DepartmentId)
                .HasDatabaseName("IX_Employees_DepartmentId");

            // Relacionamentos
            builder.HasOne<Department>()
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Employees_Departments");

            // Hierarchy relationship
            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Employees_Employees_Manager")
                .IsRequired(false); // Manager is optional

            // Configurações de auditoria são herdadas de BaseEntity
        }
    }
}
