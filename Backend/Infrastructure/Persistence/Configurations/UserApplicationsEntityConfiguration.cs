using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core mapping for <see cref="UserApplicationsEntity"/>.
    /// </summary>
    public class UserApplicationsEntityConfiguration : IEntityTypeConfiguration<UserApplicationsEntity>
    {
        public void Configure(EntityTypeBuilder<UserApplicationsEntity> builder)
        {
            builder.HasIndex(grant => new { grant.UserId, grant.ApplicationId }).IsUnique();

            builder.HasOne<UsersEntity>()
                .WithMany()
                .HasForeignKey(grant => grant.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationsEntity>()
                .WithMany()
                .HasForeignKey(grant => grant.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}