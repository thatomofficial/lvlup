using FluentValidation;

namespace LvlUp.Application.Auth.Register;

internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Surname).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(30)
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("'Username' may only contain letters, digits and underscores.");
    }
}
