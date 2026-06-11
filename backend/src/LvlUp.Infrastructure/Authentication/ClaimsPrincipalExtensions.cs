using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace LvlUp.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal? principal)
    {
        string? value = principal?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(value, out Guid userId) ? userId : null;
    }
}
