using Microsoft.AspNetCore.Authorization;

namespace LvlUp.Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Fine-grained permission checks are not implemented yet: every authenticated
        // hunter is granted all permissions. The permission infrastructure stays in
        // place so endpoints can already declare their required permissions.
        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
