using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core mapping for <see cref="UsersLogEntity"/>.
    /// </summary>
    public class UsersLogEntityConfiguration : IEntityTypeConfiguration<UsersLogEntity>
    {
        public void Configure(EntityTypeBuilder<UsersLogEntity> builder)
        {
            builder.Property(log => log.OperationType).IsRequired().HasMaxLength(16);
            builder.Property(log => log.Birthdate).HasColumnType("date");
            builder.HasIndex(log => log.RecordId);
        }
    }
}