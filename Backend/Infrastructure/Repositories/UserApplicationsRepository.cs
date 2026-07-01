using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Statements;
using System.Data;

namespace Infrastructure.Repositories
{
    public class UserApplicationsRepository(
        IDbConnection dbConnection
    )
        : IUserApplicationsRepository
    {
        public async Task<UserApplicationsEntity?> GetGrantAsync(int userId, int applicationId)
        {
            var parameters = new { UserId = userId, ApplicationId = applicationId };
            return await dbConnection.QueryFirstOrDefaultAsync<UserApplicationsEntity>(
                UserApplicationsStatements.SelectGrant, parameters
            );
        }

        public async Task<int> GrantAccessAsync(int userId, int applicationId, string roles, int operationUserId)
        {
            // Re-use the existing row (if any) so re-granting access after a revoke
            // updates it in place instead of violating the UserId/ApplicationId unique constraint.
            var existingGrant = await GetGrantAsync(userId, applicationId);

            var parameters = new DynamicParameters();
            parameters.Add("Id", existingGrant?.Id);
            parameters.Add("UserId", userId);
            parameters.Add("ApplicationId", applicationId);
            parameters.Add("Roles", roles);
            parameters.Add("IsActive", true);
            parameters.Add("OperationUserId", operationUserId);

            return await dbConnection.ExecuteScalarAsync<int>(
                UserApplicationsStatements.UpsertUserApplication, parameters, commandType: CommandType.StoredProcedure
            );
        }

        public async Task RevokeAccessAsync(int userId, int applicationId, int operationUserId)
        {
            var existingGrant = await GetGrantAsync(userId, applicationId);

            if (existingGrant is null)
                return;

            var parameters = new DynamicParameters();
            parameters.Add("Id", existingGrant.Id);
            parameters.Add("UserId", userId);
            parameters.Add("ApplicationId", applicationId);
            parameters.Add("Roles", existingGrant.Roles);
            parameters.Add("IsActive", false);
            parameters.Add("OperationUserId", operationUserId);

            await dbConnection.ExecuteScalarAsync<int>(
                UserApplicationsStatements.UpsertUserApplication, parameters, commandType: CommandType.StoredProcedure
            );
        }
    }
}