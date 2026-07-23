using Domain.Entities;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    /// <summary>
    /// EF Core write-path context for the AuthForge schema. Reads are served by Dapper against
    /// the same connection; this context only handles inserts and updates.
    /// </summary>
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<UsersEntity> Users => Set<UsersEntity>();

        public DbSet<ApplicationsEntity> Applications => Set<ApplicationsEntity>();

        public DbSet<UserApplicationsEntity> UserApplications => Set<UserApplicationsEntity>();

        public DbSet<UsersLogEntity> UsersLog => Set<UsersLogEntity>();

        public DbSet<ApplicationsLogEntity> ApplicationsLog => Set<ApplicationsLogEntity>();

        public DbSet<UserApplicationsLogEntity> UserApplicationsLog => Set<UserApplicationsLogEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        /// <summary>
        /// Saves all tracked changes and appends a matching audit log row for every inserted or
        /// updated <see cref="UsersEntity"/>, <see cref="ApplicationsEntity"/> or
        /// <see cref="UserApplicationsEntity"/>, atomically in the same transaction.
        /// </summary>
        /// <param name="operationUserId">The identity performing this operation, recorded on the audit row.</param>
        /// <param name="cancellationToken">Propagates notification that the operation should be canceled.</param>
        /// <returns>The number of state entries written to the main tables.</returns>
        public async Task<int> SaveChangesWithAuditAsync(int operationUserId, CancellationToken cancellationToken = default)
        {
            var pendingAuditEntries = ChangeTracker.Entries()
                .Where(entry => entry.State is EntityState.Added or EntityState.Modified)
                .Select(entry => (Entry: entry, OperationType: entry.State == EntityState.Added ? "INSERT" : "UPDATE"))
                .ToList();

            if (pendingAuditEntries.Count == 0)
                return await SaveChangesAsync(cancellationToken);

            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

            var affectedRows = await SaveChangesAsync(cancellationToken);

            foreach (var (entry, operationType) in pendingAuditEntries)
                AddAuditLog(entry.Entity, operationType, operationUserId);

            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return affectedRows;
        }

        private void AddAuditLog(object entity, string operationType, int operationUserId)
        {
            switch (entity)
            {
                case UsersEntity user:
                    UsersLog.Add(new UsersLogEntity
                    {
                        RecordId = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PasswordHash = user.PasswordHash,
                        Birthdate = user.Birthdate,
                        IsPlatformAdmin = user.IsPlatformAdmin,
                        OperationUserId = operationUserId,
                        OperationType = operationType,
                        IsActive = user.IsActive
                    });
                    break;

                case ApplicationsEntity application:
                    ApplicationsLog.Add(new ApplicationsLogEntity
                    {
                        RecordId = application.Id,
                        Name = application.Name,
                        ClientId = application.ClientId,
                        ClientSecret = application.ClientSecret,
                        OperationUserId = operationUserId,
                        OperationType = operationType,
                        IsActive = application.IsActive
                    });
                    break;

                case UserApplicationsEntity grant:
                    UserApplicationsLog.Add(new UserApplicationsLogEntity
                    {
                        RecordId = grant.Id,
                        UserId = grant.UserId,
                        ApplicationId = grant.ApplicationId,
                        Roles = grant.Roles,
                        OperationUserId = operationUserId,
                        OperationType = operationType,
                        IsActive = grant.IsActive
                    });
                    break;
            }
        }
    }
}