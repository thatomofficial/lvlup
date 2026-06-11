namespace LvlUp.Api.Extensions;

public static class AuthorizationExtensions
{
    public static RouteHandlerBuilder HasPermission(this RouteHandlerBuilder builder, string permission) =>
        builder.RequireAuthorization(permission);
}
