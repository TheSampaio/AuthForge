using Application.Contracts;

namespace Application.Interfaces
{
    public interface IAdminService
    {
        Task<Result<int>> RegisterAsync(RegisterRequest request);
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    }
}