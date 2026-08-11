using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthDemo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok(new { data = "This is public data, no authentication required" });
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult GetProtectedData()
        {
            var username = User.Identity?.Name;
            return Ok(new
            {
                data = "This is protected data",
                user = username,
                timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult GetAdminData()
        {
            return Ok(new
            {
                data = "This is admin-only data",
                message = "You have Admin privileges!"
            });
        }

        [HttpGet("session")]
        public IActionResult GetSessionData([FromHeader] string? sessionId)
        {
            if (string.IsNullOrEmpty(sessionId) || !sessionId.StartsWith("session_"))
                return Unauthorized(new { message = "Invalid or missing session" });

            return Ok(new
            {
                data = "Session-based authentication successful",
                sessionId = sessionId
            });
        }

        [HttpGet("api-key-data")]
        public IActionResult GetDataWithApiKey([FromHeader(Name = "X-API-Key")] string? apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                return BadRequest(new { message = "API Key is required" });

            // 这里只做演示，实际验证在中间件或 AuthService 中
            return Ok(new
            {
                data = "API Key authentication successful",
                apiKey = apiKey
            });
        }
    }
}
