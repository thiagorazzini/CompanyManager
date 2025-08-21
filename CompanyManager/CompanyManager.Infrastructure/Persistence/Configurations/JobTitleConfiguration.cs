using CompanyManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompanyManager.Infrastructure.Persistence.Configurations;

public class JobTitleConfiguration : IEntityTypeConfiguration<JobTitle>
{
    public void Configure(EntityTypeBuilder<JobTitle> builder)
    {
        builder.ToTable("JobTitles");

        builder.HasKey(jt => jt.Id);

        builder.Property(jt => jt.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Nome do cargo/função");

        builder.Property(jt => jt.HierarchyLevel)
            .IsRequired()
            .HasComment("Nível hierárquico (1 = President, 5 = Employee)");

        builder.Property(jt => jt.Description)
            .HasMaxLength(500)
            .HasComment("Descrição do cargo");

        builder.Property(jt => jt.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indica se o cargo está ativo");

        // Índices
        builder.HasIndex(jt => jt.Name).IsUnique();
        builder.HasIndex(jt => jt.HierarchyLevel);

        // Relacionamentos
        builder.HasMany(jt => jt.Employees)
            .WithOne(e => e.JobTitle)
            .HasForeignKey(e => e.JobTitleId)
            .OnDelete(DeleteBehavior.Restrict); // Não permite deletar cargo se há funcionários

        // Dados de seed serão configurados no DatabaseInitializer
    }
}

