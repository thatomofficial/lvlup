using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Auth.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string Name,
    string Surname,
    string Username) : ICommand<AuthResponse>;
