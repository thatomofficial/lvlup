using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Auth.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : ICommand;
