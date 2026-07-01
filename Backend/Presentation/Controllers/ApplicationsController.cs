using Application.Contracts;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/admin/applications")]
    public class ApplicationsController(IApplicationsService appsService) : ControllerBase
    {
        /// <summary>
        /// Registers a new application ecosystem. The creator is automatically assigned as Admin.
        /// </summary>
        /// <param name="request">The application details.</param>
        /// <returns>The generated public Client ID for the application.</returns>
        [Authorize(Policy = "CentralOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateApplicationRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out var userId))
                return Unauthorized(Result<string>.Failure("Invalid user token."));

            var result = await appsService.CreateApplicationAsync(request, userId);

            return result.IsSuccess
                ? Created(string.Empty, result)
                : BadRequest(result);
        }

        /// <summary>
        /// Grants a user access to a specific application ecosystem.
        /// </summary>
        /// <param name="request">The assignment configuration details.</param>
        /// <returns>HTTP status confirming the operation.</returns>
        [HttpPost("users")]
        public async Task<IActionResult> AssignUserAsync([FromBody] AssignUserRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out var requesterUserId))
                return Unauthorized(Result<string>.Failure("Invalid user token."));

            var result = await appsService.AssignUserAsync(request, requesterUserId);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        /// <summary>
        /// Revokes a user's access to a specific application ecosystem.
        /// </summary>
        /// <param name="clientId">The public Client ID of the application.</param>
        /// <param name="userId">The ID of the user whose access will be revoked.</param>
        /// <returns>HTTP status confirming the operation.</returns>
        [HttpDelete("{clientId:guid}/users/{userId:int}")]
        public async Task<IActionResult> RevokeUserAsync(Guid clientId, int userId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out var requesterUserId))
                return Unauthorized(Result<string>.Failure("Invalid user token."));

            var result = await appsService.RevokeUserAsync(clientId, userId, requesterUserId);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        /// <summary>
        /// Deactivates an application ecosystem, preventing further SSO logins or registrations.
        /// </summary>
        /// <param name="clientId">The public Client ID of the application.</param>
        /// <returns>HTTP status confirming the operation.</returns>
        [HttpDelete("{clientId:guid}")]
        public async Task<IActionResult> DeactivateAsync(Guid clientId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out var requesterUserId))
                return Unauthorized(Result<string>.Failure("Invalid user token."));

            var result = await appsService.DeactivateApplicationAsync(clientId, requesterUserId);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        /// <summary>
        /// Retrieves a list of applications managed by the authenticated user.
        /// </summary>
        /// <returns>A collection of applications with their Client IDs.</returns>
        [HttpGet]
        public async Task<IActionResult> GetMyApplicationsAsync()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out var userId))
                return Unauthorized(Result<string>.Failure("Invalid user token."));

            var result = await appsService.GetUserApplicationsAsync(userId);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }
    }
}