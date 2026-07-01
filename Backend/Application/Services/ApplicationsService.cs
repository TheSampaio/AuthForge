using Application.Contracts;
using Application.Interfaces;
using Domain.Interfaces;

namespace Application.Services
{
    public class ApplicationsService(
        IApplicationsRepository applicationsRepository,
        IUserApplicationsRepository userApplicationsRepository
    )
        : IApplicationsService
    {
        public async Task<Result<Guid>> CreateApplicationAsync(CreateApplicationRequest request, int requesterUserId)
        {
            var clientId = await applicationsRepository.CreateAsync(request.Name, requesterUserId);
            var application = await applicationsRepository.GetByClientIdAsync(clientId);

            if (application != null)
            {
                await userApplicationsRepository.GrantAccessAsync(requesterUserId, application.Id, "Admin", requesterUserId);
            }

            return Result<Guid>.Success(clientId);
        }

        public async Task<Result<bool>> AssignUserAsync(AssignUserRequest request, int requesterUserId)
        {
            var application = await applicationsRepository.GetByClientIdAsync(request.ClientId);

            if (application is null)
                return Result<bool>.Failure("Application not found.");

            if (request.UserId == requesterUserId && request.Role.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                await userApplicationsRepository.GrantAccessAsync(request.UserId, application.Id, "User", requesterUserId);
                return Result<bool>.Success(true);
            }

            if (!await IsAdminOfApplicationAsync(requesterUserId, application.Id))
                return Result<bool>.Failure("You do not have administrative privileges for this application.");

            await userApplicationsRepository.GrantAccessAsync(request.UserId, application.Id, request.Role, requesterUserId);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RevokeUserAsync(Guid clientId, int userId, int requesterUserId)
        {
            var application = await applicationsRepository.GetByClientIdAsync(clientId);

            if (application is null)
                return Result<bool>.Failure("Application not found.");

            var targetGrant = await userApplicationsRepository.GetGrantAsync(userId, application.Id);

            if (targetGrant is not { IsActive: true })
                return Result<bool>.Failure("User does not have access to this application.");

            if (userId != requesterUserId && !await IsAdminOfApplicationAsync(requesterUserId, application.Id))
                return Result<bool>.Failure("You do not have administrative privileges for this application.");

            await userApplicationsRepository.RevokeAccessAsync(userId, application.Id, requesterUserId);

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeactivateApplicationAsync(Guid clientId, int requesterUserId)
        {
            var application = await applicationsRepository.GetByClientIdAsync(clientId);

            if (application is null)
                return Result<bool>.Failure("Application not found.");

            if (!await IsAdminOfApplicationAsync(requesterUserId, application.Id))
                return Result<bool>.Failure("You do not have administrative privileges for this application.");

            await applicationsRepository.DeactivateAsync(application, requesterUserId);

            return Result<bool>.Success(true);
        }

        private async Task<bool> IsAdminOfApplicationAsync(int userId, int applicationId)
        {
            var grant = await userApplicationsRepository.GetGrantAsync(userId, applicationId);
            return grant is { IsActive: true } && grant.Roles?.Contains("Admin", StringComparison.OrdinalIgnoreCase) == true;
        }

        public async Task<Result<IEnumerable<ApplicationResponse>>> GetUserApplicationsAsync(int userId)
        {
            var result = await applicationsRepository.GetByUserIdAsync(userId);
            var response = result.Select(app => new ApplicationResponse(
                app.Name,
                app.ClientId
            ));

            return Result<IEnumerable<ApplicationResponse>>.Success(response);
        }
    }
}