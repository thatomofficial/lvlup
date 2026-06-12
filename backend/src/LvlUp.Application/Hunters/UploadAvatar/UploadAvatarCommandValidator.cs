using FluentValidation;

namespace LvlUp.Application.Hunters.UploadAvatar;

internal sealed class UploadAvatarCommandValidator : AbstractValidator<UploadAvatarCommand>
{
    private const int MaxSizeBytes = 5 * 1024 * 1024;

    public UploadAvatarCommandValidator()
    {
        RuleFor(command => command.HunterId).NotEmpty();

        RuleFor(command => command.Content)
            .Must(content => content.Length > 0)
            .WithMessage("An image file is required.")
            .Must(content => content.Length <= MaxSizeBytes)
            .WithMessage("The avatar must be 5 MB or smaller.");

        RuleFor(command => command.ContentType).NotEmpty();
    }
}
