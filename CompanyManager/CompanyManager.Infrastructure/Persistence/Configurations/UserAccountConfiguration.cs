using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CompanyManager.Domain.Entities;

namespace CompanyManager.Infrastructure.Persistence.Configurations
{
    public class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
    {
        public void Configure(EntityTypeBuilder<UserAccount> builder)
        {
            builder.ToTable("UserAccounts");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).ValueGeneratedNever();

            // Propriedades básicas
            // UserName é configurado como Value Object no CompanyContext
            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("Hash da senha");

            builder.Property(u => u.SecurityStamp)
                .IsRequired()
                .HasComment("Stamp de segurança para invalidar tokens");

            // CreatedAt e UpdatedAt são herdados de BaseEntity
            // PasswordChangedAt é configurado automaticamente
            // AccessFailedCount é configurado automaticamente
            // LockoutEndUtc é configurado automaticamente
            // TwoFactorEnabled e TwoFactorSecret são configurados automaticamente
            // EmployeeId é configurado automaticamente

            // Índices
            // UserName é configurado como Value Object no CompanyContext
            builder.HasIndex(u => u.SecurityStamp)
                .HasDatabaseName("IX_UserAccounts_SecurityStamp");

            // Relacionamentos
            builder.HasMany(u => u.Roles)
                .WithMany()
                .UsingEntity(j => j.ToTable("UserAccountRoles"));

            // Configurações de auditoria são herdadas de BaseEntity
        }
    }
}
