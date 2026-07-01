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
                Birthdate = request.Birthdate
            };

            var userId = await usersRepository.CreateAsync(newUser);
            return Result<int>.Success(userId);
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var user = await usersRepository.GetByEmailAsync(request.Email);

            if (user is null || !cryptoService.VerifyPassword(request.Password, user.PasswordHash))
                return Result<LoginResponse>.Failure("Invalid email or password.");

            var token = jwtService.GenerateToken(user);
            var expirationInMinutes = int.Parse(configuration["JwtSettings:ExpirationInMinutes"] ?? "60");

            return Result<LoginResponse>.Success(new LoginResponse(user.Email, token, expirationInMinutes));
        }
    }
}