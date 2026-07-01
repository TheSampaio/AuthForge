using Application.Contracts;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AdminController(IAdminService adminService) : ControllerBase
    {
        /// <summary>
        /// Authenticates the central platform identity and issues a central JWT token.
        /// </summary>
        /// <param name="request">User credentials.</param>
        /// <returns>A JWT token and user details if successful.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            var result = await adminService.LoginAsync(request);
            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }

        /// <summary>
        /// Registers a new central platform identity, used to manage applications.
        /// </summary>
        /// <param name="request">User details for registration.</param>
        /// <returns>The ID of the newly created user.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            var result = await adminService.RegisterAsync(request);
            return result.IsSuccess ? Created(string.Empty, result) : BadRequest(result);
        }
    }
}