using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserApplicationsRepository
    {
        Task<UserApplicationsEntity?> GetGrantAsync(int userId, int applicationId);
        Task<int> GrantAccessAsync(int userId, int applicationId, string roles, int operationUserId);
        Task RevokeAccessAsync(int userId, int applicationId, int operationUserId);
    }
}