using LvlUp.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace LvlUp.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId =>
        httpContextAccessor.HttpContext?.User.GetUserId() ??
        throw new InvalidOperationException("The user context is unavailable.");
}
