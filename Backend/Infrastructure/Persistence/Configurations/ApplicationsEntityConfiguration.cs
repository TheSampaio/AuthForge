using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core mapping for <see cref="ApplicationsEntity"/>.
    /// </summary>
    public class ApplicationsEntityConfiguration : IEntityTypeConfiguration<ApplicationsEntity>
    {
        public void Configure(EntityTypeBuilder<ApplicationsEntity> builder)
        {
            builder.HasIndex(a => a.Name).IsUnique();
            builder.HasIndex(a => a.ClientId).IsUnique();
        }
    }
}