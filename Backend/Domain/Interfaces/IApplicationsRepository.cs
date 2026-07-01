using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IApplicationsRepository
    {
        Task<ApplicationsEntity?> GetByIdAsync(int id);
        Task<ApplicationsEntity?> GetByClientIdAsync(Guid clientId);
        Task<IEnumerable<ApplicationsEntity>> GetByUserIdAsync(int userId);
        Task<Guid> CreateAsync(string name, int operationUserId);
        Task DeactivateAsync(ApplicationsEntity application, int operationUserId);
    }
}