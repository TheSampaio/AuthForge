using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Authorize(Policy = "CentralOnly")]
    [ApiController]
    [Route("api/v1/admin/users")]
    public class UsersController(
        IUsersService usersService
    )
        : ControllerBase
    {
        /// <summary>
        /// Retrieves a list of all active users.
        /// </summary>
        /// <returns>A collection of user records.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await usersService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>The requested user record, or HTTP 404 if not found.</returns>
        [HttpGet("{email}")]
        public async Task<IActionResult> GetByEmailAsync(string email)
        {
            var result = await usersService.GetByEmailAsync(email);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}