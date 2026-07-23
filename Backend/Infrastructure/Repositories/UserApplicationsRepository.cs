using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Statements;
using System.Data;

namespace Infrastructure.Repositories
{
    public class UserApplicationsRepository(
        IDbConnection dbConnection,
        AppDbContext dbContext
    )
        : IUserApplicationsRepository
    {
        public async Task<UserApplicationsEntity?> GetGrantAsync(int userId, int applicationId)
        {
            return await dbConnection.QueryFirstOrDefaultAsync<UserApplicationsEntity>(
                UserApplicationsStatements.SelectGrant, new { UserId = userId, ApplicationId = applicationId }
            );
        }

        public async Task<int> GrantAccessAsync(int userId, int applicationId, string roles, int operationUserId)
        {
            // Re-use the existing row (if any) so re-granting access after a revoke updates it
            // in place instead of violating the (user_id, application_id) unique constraint.
            var existingGrant = await GetGrantAsync(userId, applicationId);

            if (existingGrant is null)
            {
                var newGrant = new UserApplicationsEntity
                {
                    UserId = userId,
                    ApplicationId = applicationId,
                    Roles = roles,
                    IsActive = true
                };

                dbContext.UserApplications.Add(newGrant);
                await dbContext.SaveChangesWithAuditAsync(operationUserId);

                return newGrant.Id;
            }

            existingGrant.Roles = roles;
            existingGrant.IsActive = true;

            dbContext.UserApplications.Update(existingGrant);
            await dbContext.SaveChangesWithAuditAsync(operationUserId);

            return existingGrant.Id;
        }

        public async Task RevokeAccessAsync(int userId, int applicationId, int operationUserId)
        {
            var existingGrant = await GetGrantAsync(userId, applicationId);

            if (existingGrant is null)
                return;

            existingGrant.IsActive = false;

            dbContext.UserApplications.Update(existingGrant);
            await dbContext.SaveChangesWithAuditAsync(operationUserId);
        }
    }
}