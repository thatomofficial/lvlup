using Microsoft.AspNetCore.Authorization;

namespace LvlUp.Infrastructure.Authorization;

internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
