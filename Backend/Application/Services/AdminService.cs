using Application.Contracts;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class AdminService(
        IUsersRepository usersRepository,
        ICryptoService cryptoService,
        IJwtService jwtService,
        IConfiguration configuration
    )
        : IAdminService
    {
        public async Task<Result<int>> RegisterAsync(RegisterRequest request)
        {
            // The platform has exactly one central admin. Once it exists, this endpoint must
            // never create another central identity, or anyone who finds the route could grant
            // themselves full control over every registered application.
            if (await usersRepository.ExistsPlatformAdminAsync())
                return Result<int>.Failure("Platform admin has already been configured.");

            var existingUser = await usersRepository.GetByEmailAsync(request.Email);

            if (existingUser is not null)
                return Result<int>.Failure("Email is already in use.");

            var hashedPassword = cryptoService.HashPassword(request.Password);

            var newUser = new UsersEntity
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Birthdate = request.Birthdate,
                IsPlatformAdmin = true
            };

            var userId = await usersRepository.CreateAsync(newUser);
            return Result<int>.Success(userId);
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var user = await usersRepository.GetByEmailAsync(request.Email);

            // Users is shared with per-application SSO identities; without this check, any
            // end user of any connected application could log in here and receive a central
            // token, since their credentials alone would otherwise pass verification.
            if (user is null || !user.IsPlatformAdmin || !cryptoService.VerifyPassword(request.Password, user.PasswordHash))
                return Result<LoginResponse>.Failure("Invalid email or password.");

            var token = jwtService.GenerateToken(user);
            var expirationInMinutes = int.Parse(configuration["JwtSettings:ExpirationInMinutes"] ?? "60");

            return Result<LoginResponse>.Success(new LoginResponse(user.Email, token, expirationInMinutes));
        }
    }
}