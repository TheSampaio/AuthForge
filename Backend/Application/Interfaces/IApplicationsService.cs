using Application.Contracts;

namespace Application.Interfaces
{
    public interface IApplicationsService
    {
        Task<Result<Guid>> CreateApplicationAsync(CreateApplicationRequest request, int requesterUserId);
        Task<Result<bool>> AssignUserAsync(AssignUserRequest request, int requesterUserId);
        Task<Result<bool>> RevokeUserAsync(Guid clientId, int userId, int requesterUserId);
        Task<Result<bool>> DeactivateApplicationAsync(Guid clientId, int requesterUserId);
        Task<Result<IEnumerable<ApplicationResponse>>> GetUserApplicationsAsync(int userId);
    }
}