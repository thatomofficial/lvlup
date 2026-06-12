using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Hunters.UpdateGitHubUsername;

public sealed record UpdateGitHubUsernameCommand(Guid HunterId, string? Username) : ICommand;
