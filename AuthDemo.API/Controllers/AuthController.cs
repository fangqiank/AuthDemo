using AuthDemo.Core.Models;
using AuthDemo.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthDemo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IAuthService authService, 
        IJwtService jwtService
        ) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await authService.AuthenticateAsync(request.Username, request.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var accessToken = jwtService.GenerateAccessToken(user);
            var refreshToken = jwtService.GenerateRefreshToken(user);

            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 900 // 15 minutes
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await jwtService.RefreshTokenAsync(request.RefreshToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }
        }

        [HttpPost("api-key")]
        public async Task<IActionResult> AuthenticateWithApiKey([FromBody] ApiKeyRequest request)
        {
            var user = await authService.GetUserByApiKeyAsync(request.ApiKey);
            if (user == null)
                return Unauthorized(new { message = "Invalid API key" });

            var token = jwtService.GenerateAccessToken(user);
            return Ok(new { accessToken = token, user = user.Username });
        }

        [Authorize]
        [HttpGet("validate")]
        public IActionResult Validate()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Ok(new
            {
                message = "Token is valid",
                userId = userId,
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // 实际项目中应使 Refresh Token 失效
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
