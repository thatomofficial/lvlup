using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Auth.LoginWithGoogle;

public sealed record LoginWithGoogleCommand(string IdToken) : ICommand<AuthResponse>;
