using AspNet.Security.OAuth.GitHub;
using AuthDemoAgain.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthDemoAgain.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
           var user = MockUserStore.Users.FirstOrDefault(u => u.Username == request.Username 
            && u.Password == request.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("department", user.Department) // ABAC 用户属性 → claim
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity));

            return Ok($"登录成功，当前角色: {user.Role}");
        }

        // GitHub OAuth：浏览器访问此端点 → 302 到 GitHub 授权页；
        // GitHub 回调 /signin-github 后自动签发 Cookie 并跳转到 profile
        [HttpGet("github-login")]
        public IActionResult GitHubLogin()
            => Challenge(new AuthenticationProperties { RedirectUri = "/" },
                GitHubAuthenticationDefaults.AuthenticationScheme);

        // SPA 身份探测：不加 [Authorize]，未登录返回 null（否则会被 Challenge 重定向拿到 HTML）
        [HttpGet("me")]
        public IActionResult Me()
        {
            if (User.Identity?.IsAuthenticated != true) return Ok(null);
            return Ok(new
            {
                name = User.Identity?.Name,
                role = User.FindFirst(ClaimTypes.Role)?.Value,
                department = User.FindFirst("department")?.Value
            });
        }

        // OIDC (Keycloak)：浏览器访问 → 302 到 Keycloak 登录页；
        // 回调 /signin-oidc（校验 ID Token、换 token）后自动签发 Cookie 并跳回
        [HttpGet("oidc-login")]
        public IActionResult OidcLogin()
            => Challenge(new AuthenticationProperties { RedirectUri = "/" },
                OpenIdConnectDefaults.AuthenticationScheme);

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("已退出登录");
        }

        [Authorize(Policy = "UserOrAdmin")]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var username = User.Identity?.Name;
            return Ok($"你好，{username}！你拥有普通权限。");
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin-data")]
        public IActionResult GetAdminInfo()
        {
            return Ok("这是管理员专属信息。");
        }

        // ABAC：不看好角色，看用户属性 department
        [Authorize(Policy = "ItDepartmentOnly")]
        [HttpGet("abac-data")]
        public IActionResult GetAbacData()
        {
            var dept = User.FindFirst("department")?.Value;
            return Ok($"这是 ABAC 保护的资源，仅 IT 部门可见。你的部门: {dept}");
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
