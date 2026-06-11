using FluentValidation;

namespace LvlUp.Application.Auth.Register;

internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(50);
    }
}
