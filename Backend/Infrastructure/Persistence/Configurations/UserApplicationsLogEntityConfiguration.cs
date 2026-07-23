using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core mapping for <see cref="UserApplicationsLogEntity"/>.
    /// </summary>
    public class UserApplicationsLogEntityConfiguration : IEntityTypeConfiguration<UserApplicationsLogEntity>
    {
        public void Configure(EntityTypeBuilder<UserApplicationsLogEntity> builder)
        {
            builder.Property(log => log.OperationType).IsRequired().HasMaxLength(16);
            builder.HasIndex(log => log.RecordId);
        }
    }
}