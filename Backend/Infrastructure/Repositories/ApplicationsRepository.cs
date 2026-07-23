using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Statements;
using System.Data;

namespace Infrastructure.Repositories
{
    public class ApplicationsRepository(
        IDbConnection dbConnection,
        AppDbContext dbContext
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
            var application = new ApplicationsEntity
            {
                Name = name,
                ClientId = Guid.NewGuid()
            };

            dbContext.Applications.Add(application);
            await dbContext.SaveChangesWithAuditAsync(operationUserId);

            return application.ClientId;
        }

        public async Task DeactivateAsync(ApplicationsEntity application, int operationUserId)
        {
            application.IsActive = false;

            dbContext.Applications.Update(application);
            await dbContext.SaveChangesWithAuditAsync(operationUserId);
        }
    }
}