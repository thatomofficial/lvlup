using LvlUp.SharedKernel;

namespace LvlUp.Domain.Hunters;

public static class HunterErrors
{
    public static readonly Error EmailNotUnique = Error.Conflict(
        "Hunters.EmailNotUnique",
        "A hunter with the provided email already exists.");

    public static readonly Error UsernameNotUnique = Error.Conflict(
        "Hunters.UsernameNotUnique",
        "A hunter with the provided username already exists.");

    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Hunters.InvalidCredentials",
        "The provided credentials are invalid.");

    public static Error NotFound(Guid hunterId) => Error.NotFound(
        "Hunters.NotFound",
        $"The hunter with the identifier '{hunterId}' was not found.");
}
