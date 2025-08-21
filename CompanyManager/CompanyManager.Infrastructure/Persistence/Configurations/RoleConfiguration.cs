using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CompanyManager.Domain.AccessControl;

namespace CompanyManager.Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedNever();

            // Propriedades básicas
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Nome da role");

            // Configurar a propriedade Level
            builder.Property(r => r.Level)
                .IsRequired()
                .HasConversion<int>()
                .HasComment("Nível hierárquico da role (1=Junior, 2=Pleno, 3=Senior, 4=Manager, 5=Director, 999=SuperUser)");

            builder.Property(r => r.Permissions)
                .HasConversion(
                    v => string.Join(";", v),
                    v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase))
                .HasMaxLength(1000)
                .HasComment("Lista de permissões separadas por ponto e vírgula");

            // CreatedAt e UpdatedAt são herdados de BaseEntity

            // Índices
            builder.HasIndex(r => r.Name)
                .IsUnique()
                .HasDatabaseName("IX_Roles_Name");

            // Configurações de auditoria são herdadas de BaseEntity
        }
    }
}
