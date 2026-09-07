using AuthDemoAgain.Models;
using Microsoft.AspNetCore.Authorization;

namespace AuthDemoAgain.Authorization
{
    // 资源型授权需求：文档 Owner == 当前用户才通过。
    // 判断收敛在 Handler，端点只调 IAuthorizationService.AuthorizeAsync 传入具体资源，不写 if。
    public class DocumentOwnerRequirement : IAuthorizationRequirement
    {
    }

    public class DocumentOwnerHandler : AuthorizationHandler<DocumentOwnerRequirement, Document>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DocumentOwnerRequirement requirement, Document resource)
        {
            if (context.User.Identity?.Name == resource.Owner)
                context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
