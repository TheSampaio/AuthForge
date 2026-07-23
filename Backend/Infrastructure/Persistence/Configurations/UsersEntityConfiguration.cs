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

            // A birthdate has no time-of-day or timezone meaning; mapping it to a plain "date"
            // avoids Npgsql's requirement that "timestamp with time zone" values be UTC-kind,
            // which client-supplied DateTimes without an explicit offset never are.
            builder.Property(u => u.Birthdate).HasColumnType("date");
        }
    }
}