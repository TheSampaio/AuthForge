using Application.Contracts;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> LoginAsync(SsoLoginRequest request);
        Task<Result<LoginResponse>> RegisterAsync(SsoRegisterRequest request);
    }
}