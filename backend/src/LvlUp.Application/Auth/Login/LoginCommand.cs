using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthResponse>;
