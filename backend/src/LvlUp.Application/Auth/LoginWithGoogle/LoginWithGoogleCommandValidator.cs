using FluentValidation;

namespace LvlUp.Application.Auth.LoginWithGoogle;

internal sealed class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(command => command.IdToken).NotEmpty();
    }
}
