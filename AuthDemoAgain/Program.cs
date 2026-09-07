using AspNet.Security.OAuth.GitHub;
using AuthDemoAgain.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 1. 配置认证 (Authentication): 回答“你是谁”
// 单体应用选择 Session (基于 Cookie)，因为它在服务端可控，可随时注销。
// 加入 GitHub OAuth 后需同时指定 DefaultScheme (Cookie) 和 DefaultChallengeScheme (GitHub)：
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/api/auth/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["GitHub:ClientId"]
            ?? throw new InvalidOperationException("缺少 GitHub:ClientId，请先执行 dotnet user-secrets set \"GitHub:ClientId\" <你的ClientId>");
        options.ClientSecret = builder.Configuration["GitHub:ClientSecret"]
            ?? throw new InvalidOperationException("缺少 GitHub:ClientSecret，请先执行 dotnet user-secrets set \"GitHub:ClientSecret\" <你的ClientSecret>");
        options.CallbackPath = "/signin-github";

        // GitHub 身份 → 补齐与本地登录相同的 claims（默认 Role/department），
        // 使现有 RBAC/ABAC 端点对 OAuth 用户直接生效。
        options.Events.OnCreatingTicket = ctx =>
        {
            if (ctx.Principal?.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                identity.AddClaim(new Claim("department", "General"));
            }
            return Task.CompletedTask;
        };
    })
    // OIDC (Keycloak)：OAuth 2.0 之上的身份层——IdP 签发 ID Token (JWT) 证明“你是谁”。
    // Keycloak 不支持 OAuth2 登录而支持 OIDC，故与 GitHub 是两个独立方案。
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"]
            ?? throw new InvalidOperationException("缺少 Keycloak:Authority（形如 http://192.168.1.138:8080/realms/<realm>）");
        options.ClientId = builder.Configuration["Keycloak:ClientId"]
            ?? throw new InvalidOperationException("缺少 Keycloak:ClientId");
        options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"]
            ?? throw new InvalidOperationException("缺少 Keycloak:ClientSecret（Keycloak 客户端需开启 Client authentication）");
        options.ResponseType = OpenIdConnectResponseType.Code;   // Authorization Code 流程
        options.CallbackPath = "/signin-oidc";
        options.GetClaimsFromUserInfoEndpoint = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username"                 // Keycloak 用户名字段
        };
        options.RequireHttpsMetadata = false;                    // ponytail: Keycloak 跑 http 才需要；生产 IdP 一律 https，删掉此行

        // 与 GitHub 相同：补默认 claims，使现有 RBAC/ABAC 端点对 OIDC 用户生效
        options.Events.OnTokenValidated = ctx =>
        {
            if (ctx.Principal?.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
                identity.AddClaim(new Claim("department", "General"));
            }
            return Task.CompletedTask;
        };
    });

// 2. 配置鉴权 (Authorization): 回答“你能去哪”
// 采用 RBAC，通过 Policy (策略) 将 Role 转化为权限控制点。
builder.Services.AddSingleton<IAuthorizationHandler, DepartmentHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, DocumentOwnerHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User","Admin"));

    // 3. ABAC：基于用户属性 (department claim) 的 Policy，与 RBAC 并存
    options.AddPolicy("ItDepartmentOnly", policy => policy.Requirements.Add(new DepartmentRequirement("IT")));

    // 4. 资源型授权：判定依赖具体资源实例，端点用 IAuthorizationService 传入资源
    options.AddPolicy("DocumentOwner", policy => policy.Requirements.Add(new DocumentOwnerRequirement()));
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseDefaultFiles();   // wwwroot/index.html 作为站点首页
app.UseStaticFiles();    // 托管 SPA（wwwroot 下静态文件默认匿名可访问）

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();


