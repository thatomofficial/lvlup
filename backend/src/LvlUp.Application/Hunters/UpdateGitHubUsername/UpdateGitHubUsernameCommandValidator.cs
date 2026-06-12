using FluentValidation;

namespace LvlUp.Application.Hunters.UpdateGitHubUsername;

internal sealed class UpdateGitHubUsernameCommandValidator : AbstractValidator<UpdateGitHubUsernameCommand>
{
    public UpdateGitHubUsernameCommandValidator()
    {
        RuleFor(command => command.HunterId).NotEmpty();

        When(command => !string.IsNullOrWhiteSpace(command.Username), () =>
            RuleFor(command => command.Username!)
                .Matches("^[a-zA-Z0-9](?:[a-zA-Z0-9]|-(?=[a-zA-Z0-9])){0,38}$")
                .WithMessage("'Username' must be a valid GitHub username."));
    }
}
