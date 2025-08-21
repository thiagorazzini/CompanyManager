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

            // RoleId - agora é uma coluna direta
            builder.Property(u => u.RoleId)
                .IsRequired()
                .HasComment("ID da role do usuário");

            // JobTitleId - link para o cargo do usuário
            builder.Property(u => u.JobTitleId)
                .IsRequired()
                .HasComment("ID do cargo do usuário");

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

            // Índice para RoleId para melhorar performance de consultas
            builder.HasIndex(u => u.RoleId)
                .HasDatabaseName("IX_UserAccounts_RoleId");

            // Índice para JobTitleId para melhorar performance de consultas
            builder.HasIndex(u => u.JobTitleId)
                .HasDatabaseName("IX_UserAccounts_JobTitleId");

            // Relacionamento com Role (one-to-one)
            builder.HasOne<CompanyManager.Domain.AccessControl.Role>()
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Relacionamento com JobTitle (one-to-one)
            builder.HasOne<CompanyManager.Domain.Entities.JobTitle>()
                .WithMany()
                .HasForeignKey(u => u.JobTitleId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Configurações de auditoria são herdadas de BaseEntity
        }
    }
}
