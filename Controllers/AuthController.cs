 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store_Managment.Models;
using Store_Managment.Services;
using System.Security.Claims;

// Use your own LoginRequest model explicitly

namespace Store_Managment.Controllers
{
    [Route("AuthLogin")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Constructor Injection
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: /AuthLogin/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(
            [FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                return Unauthorized(new
                {
                    Message = "Invalid Email or Password"
                });
            }

            return Ok(result);
        }

        // POST: /AuthLogin/Logout
        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout(
            [FromBody] Logout request)
        {
            bool result = await _authService.LogoutAsync(request);

            if (!result)
            {
                return BadRequest(
                    "Invalid or already revoked refresh token.");
            }

            return Ok("Logout successful.");
        }

        // POST: /AuthLogin/refresh-token
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequest request)
        {
            var response =
                await _authService.RefreshTokenAsync(request);

            if (response == null)
            {
                return Unauthorized(new
                {
                    Message = "Invalid or expired refresh token."
                });
            }

            return Ok(response);
        }

        // POST: /AuthLogin/Forgot-password
        [HttpPost("Forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgetPass request)
        {
            await _authService.ForgotPasswordAsync(request);

            return Ok(new
            {
                Message = "If your email is registered, you will receive a password reset link."
            });
        }

        // POST: /AuthLogin/Reset-password
        [HttpPost("Reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPassword request)
        {
            bool result =
                await _authService.ResetPasswordAsync(request);

            if (!result)
            {
                return BadRequest(
                    "Invalid or expired reset token.");
            }

            return Ok("Password reset successfully.");
        }

        // POST: /AuthLogin/Change-password
        [HttpPost("Change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] Changepassword request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required.");

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("New password and confirm password do not match.");

            bool result =
                await _authService.ChangePasswordAsync(request);

            if (!result)
            {
                return BadRequest(
                    "Current password is incorrect or password update failed.");
            }

            return Ok("Password changed successfully.");
        }

        // GET: /AuthLogin/Me
        [HttpGet("Me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid user token.");
            }

            UserLoggedin? user =
                await _authService.GetCurrentUserAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }

        // GET: /AuthLogin/Permissions
        [HttpGet("Permissions")]
        [Authorize]
        public async Task<IActionResult> GetPermissions()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid user token.");
            }

            List<UserPermission> permissions =
                await _authService.GetPermissionsAsync(userId);

            return Ok(permissions);
        }

        // POST: /AuthLogin/Register
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] Register request)
        {
            int userId =
                await _authService.RegisterAsync(request);

            if (userId == -1)
            {
                return Conflict("Email already exists.");
            }

            if (userId <= 0)
            {
                return StatusCode(500,
                    "Registration could not be completed.");
            }

            return Ok(new
            {
                Message = "Registration successful.",
                UserId = userId
            });
        }
    }
}

