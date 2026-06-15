using FluentValidation;

namespace LvlUp.Application.Auth.RequestPasswordReset;

internal sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
    }
}
