using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CompanyManager.Domain.Entities;

namespace CompanyManager.Infrastructure.Persistence.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments");

            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).ValueGeneratedNever();

            // Propriedades básicas
            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Nome do departamento");

            builder.Property(d => d.Description)
                .HasMaxLength(500)
                .HasComment("Descrição do departamento");

            builder.Property(d => d.CreatedAt)
                .IsRequired()
                .HasComment("Data de criação do registro");

            builder.Property(d => d.UpdatedAt)
                .HasComment("Data da última atualização");

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Indica se o departamento está ativo");

            // Índices
            builder.HasIndex(d => d.Name)
                .IsUnique()
                .HasDatabaseName("IX_Departments_Name");

            // Configurações de auditoria
            builder.Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(d => d.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
