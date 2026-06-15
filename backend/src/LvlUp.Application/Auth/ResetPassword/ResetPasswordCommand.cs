using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Auth.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Code, string NewPassword) : ICommand;
