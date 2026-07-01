using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Statements;
using System.Data;

namespace Infrastructure.Repositories
{
    public class ApplicationsRepository(
        IDbConnection dbConnection
    )
        : IApplicationsRepository
    {
        public async Task<ApplicationsEntity?> GetByIdAsync(int id)
        {
            return await dbConnection.QueryFirstOrDefaultAsync<ApplicationsEntity>(
                ApplicationsStatements.SelectById, new { Id = id }
            );
        }

        public async Task<ApplicationsEntity?> GetByClientIdAsync(Guid clientId)
        {
            return await dbConnection.QueryFirstOrDefaultAsync<ApplicationsEntity>(
                ApplicationsStatements.SelectByClientId, new { ClientId = clientId }
            );
        }

        public async Task<IEnumerable<ApplicationsEntity>> GetByUserIdAsync(int userId)
        {
            return await dbConnection.QueryAsync<ApplicationsEntity>(
                ApplicationsStatements.SelectByUserId, new { UserId = userId }
            );
        }

        public async Task<Guid> CreateAsync(string name, int operationUserId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Name", name);
            parameters.Add("OperationUserId", operationUserId);

            var id = await dbConnection.ExecuteScalarAsync<int>(
                ApplicationsStatements.UpsertApplication, parameters, commandType: CommandType.StoredProcedure
            );

            return await dbConnection.ExecuteScalarAsync<Guid>(
                ApplicationsStatements.SelectClientIdById, new { Id = id }
            );
        }

        public async Task DeactivateAsync(ApplicationsEntity application, int operationUserId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Id", application.Id);
            parameters.Add("Name", application.Name);
            parameters.Add("ClientId", application.ClientId);
            parameters.Add("ClientSecret", application.ClientSecret);
            parameters.Add("IsActive", false);
            parameters.Add("OperationUserId", operationUserId);

            await dbConnection.ExecuteScalarAsync<int>(
                ApplicationsStatements.UpsertApplication, parameters, commandType: CommandType.StoredProcedure
            );
        }
    }
}