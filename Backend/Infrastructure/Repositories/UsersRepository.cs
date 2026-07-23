using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Statements;
using System.Data;

namespace Infrastructure.Repositories
{
    public class UsersRepository(
        IDbConnection dbConnection,
        AppDbContext dbContext
    )
        : IUsersRepository
    {
        public async Task<IEnumerable<UsersEntity>> GetAllAsync()
        {
            return await dbConnection.QueryAsync<UsersEntity>(UsersStatements.SelectAll);
        }

        public async Task<UsersEntity?> GetByIdAsync(int id)
        {
            return await dbConnection.QueryFirstOrDefaultAsync<UsersEntity>(
                UsersStatements.SelectById, new { Id = id }
            );
        }

        public async Task<UsersEntity?> GetByEmailAsync(string email)
        {
            return await dbConnection.QueryFirstOrDefaultAsync<UsersEntity>(
                UsersStatements.SelectByEmail, new { Email = email }
            );
        }

        public async Task<int> CreateAsync(UsersEntity user)
        {
            dbContext.Users.Add(user);

            // No authenticated actor exists yet for a self-registration; the record being
            // created is its own operation source, matching the previous stored-procedure behavior.
            await dbContext.SaveChangesWithAuditAsync(operationUserId: 0);

            return user.Id;
        }
    }
}