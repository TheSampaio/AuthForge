using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core mapping for <see cref="ApplicationsLogEntity"/>.
    /// </summary>
    public class ApplicationsLogEntityConfiguration : IEntityTypeConfiguration<ApplicationsLogEntity>
    {
        public void Configure(EntityTypeBuilder<ApplicationsLogEntity> builder)
        {
            builder.Property(log => log.OperationType).IsRequired().HasMaxLength(16);
            builder.HasIndex(log => log.RecordId);
        }
    }
}