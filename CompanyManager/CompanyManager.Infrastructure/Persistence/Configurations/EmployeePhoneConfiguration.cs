using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CompanyManager.Domain.Entities;
using CompanyManager.Domain.ValueObjects;

namespace CompanyManager.Infrastructure.Persistence.Configurations
{
    public class EmployeePhoneConfiguration : IEntityTypeConfiguration<EmployeePhone>
    {
        public void Configure(EntityTypeBuilder<EmployeePhone> builder)
        {
            builder.ToTable("EmployeePhones");

            builder.HasKey(ep => ep.Id);
            builder.Property(ep => ep.Id).ValueGeneratedNever();

            // Propriedades básicas
            builder.Property(ep => ep.EmployeeId)
                .IsRequired()
                .HasComment("ID do funcionário proprietário do telefone");

            builder.Property(ep => ep.Type)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Mobile")
                .HasComment("Tipo do telefone (Mobile, Work, Home, Other)");

            builder.Property(ep => ep.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Indica se é o telefone principal do funcionário");

            // Configurar PhoneNumber como Value Object
            builder.Property(ep => ep.PhoneNumber)
                .HasConversion(
                    phoneNumber => phoneNumber.Raw,
                    value => new PhoneNumber(value, "BR"))
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("PhoneNumber")
                .HasComment("Número do telefone no formato original");

            // Índices
            builder.HasIndex(ep => ep.EmployeeId)
                .HasDatabaseName("IX_EmployeePhones_EmployeeId");

            builder.HasIndex(ep => new { ep.EmployeeId, ep.IsPrimary })
                .HasDatabaseName("IX_EmployeePhones_EmployeeId_IsPrimary")
                .HasFilter("[IsPrimary] = 1")
                .IsUnique();

            // Relacionamento com Employee
            builder.HasOne(ep => ep.Employee)
                .WithMany(e => e.Phones)
                .HasForeignKey(ep => ep.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EmployeePhones_Employees_EmployeeId");

            // Configurações de auditoria são herdadas de BaseEntity
        }
    }
}
