using Microsoft.AspNetCore.Authorization;

namespace AuthDemoAgain.Authorization
{
    // ABAC 需求：要求用户的 department 属性（claim）匹配指定部门
    public class DepartmentRequirement : IAuthorizationRequirement
    {
        public string Department { get; }
        public DepartmentRequirement(string department) => Department = department;
    }

    public class DepartmentHandler : AuthorizationHandler<DepartmentRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DepartmentRequirement requirement)
        {
            if (context.User.HasClaim("department", requirement.Department))
                context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
