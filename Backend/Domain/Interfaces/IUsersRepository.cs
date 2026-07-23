using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUsersRepository
    {
        Task<IEnumerable<UsersEntity>> GetAllAsync();
        Task<UsersEntity?> GetByIdAsync(int id);
        Task<UsersEntity?> GetByEmailAsync(string email);
        Task<int> CreateAsync(UsersEntity user);
        Task<bool> ExistsPlatformAdminAsync();
    }
}
