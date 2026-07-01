using Application.Contracts;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class AuthService(
        IUsersRepository usersRepository,
        IApplicationsRepository applicationsRepository,
        IUserApplicationsRepository userApplicationsRepository,
        ICryptoService cryptoService,
        IJwtService jwtService,
        IConfiguration configuration
    )
        : IAuthService
    {
        public async Task<Result<LoginResponse>> LoginAsync(SsoLoginRequest request)
        {
            var user = await usersRepository.GetByEmailAsync(request.Email);

            if (user is null || !cryptoService.VerifyPassword(request.Password, user.PasswordHash))
                return Result<LoginResponse>.Failure("Invalid email or password.");

            var application = await applicationsRepository.GetByClientIdAsync(request.ClientId);

            if (application is null || !application.IsActive)
                return Result<LoginResponse>.Failure("Invalid or inactive application.");

            var userAppGrant = await userApplicationsRepository.GetGrantAsync(user.Id, application.Id);

            if (userAppGrant is not { IsActive: true })
                return Result<LoginResponse>.Failure("User does not have permission to access this application.");

            var token = jwtService.GenerateToken(user, application.Name, userAppGrant.Roles);
            var expirationInMinutes = int.Parse(configuration["JwtSettings:ExpirationInMinutes"] ?? "60");

            return Result<LoginResponse>.Success(new LoginResponse(user.Email, token, expirationInMinutes));
        }

        public async Task<Result<LoginResponse>> RegisterAsync(SsoRegisterRequest request)
        {
            var application = await applicationsRepository.GetByClientIdAsync(request.ClientId);

            if (application is null || !application.IsActive)
                return Result<LoginResponse>.Failure("Invalid or inactive application.");

            var existingUser = await usersRepository.GetByEmailAsync(request.Email);

            if (existingUser is not null)
                return await JoinExistingUserAsync(existingUser, application, request.Password);

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
            newUser.Id = userId;

            const string defaultRole = "User";
            await userApplicationsRepository.GrantAccessAsync(userId, application.Id, defaultRole, userId);

            var token = jwtService.GenerateToken(newUser, application.Name, defaultRole);
            var expirationInMinutes = int.Parse(configuration["JwtSettings:ExpirationInMinutes"] ?? "60");

            return Result<LoginResponse>.Success(new LoginResponse(newUser.Email, token, expirationInMinutes));
        }

        // Handles a user who already has a central account (e.g. registered for another
        // application) "registering" for this one: verify their password and just grant access,
        // instead of rejecting with "email already in use".
        private async Task<Result<LoginResponse>> JoinExistingUserAsync(UsersEntity user, ApplicationsEntity application, string password)
        {
            if (!cryptoService.VerifyPassword(password, user.PasswordHash))
                return Result<LoginResponse>.Failure("Invalid email or password.");

            var existingGrant = await userApplicationsRepository.GetGrantAsync(user.Id, application.Id);

            if (existingGrant is { IsActive: true })
                return Result<LoginResponse>.Failure("User already has access to this application.");

            const string defaultRole = "User";
            var roles = existingGrant?.Roles ?? defaultRole;
            await userApplicationsRepository.GrantAccessAsync(user.Id, application.Id, roles, user.Id);

            var token = jwtService.GenerateToken(user, application.Name, roles);
            var expirationInMinutes = int.Parse(configuration["JwtSettings:ExpirationInMinutes"] ?? "60");

            return Result<LoginResponse>.Success(new LoginResponse(user.Email, token, expirationInMinutes));
        }
    }
}