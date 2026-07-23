using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core mapping for <see cref="UsersEntity"/>.
    /// </summary>
    public class UsersEntityConfiguration : IEntityTypeConfiguration<UsersEntity>
    {
        public void Configure(EntityTypeBuilder<UsersEntity> builder)
        {
            builder.HasIndex(u => u.Email).IsUnique();
        }
    }
}