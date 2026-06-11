using FluentValidation;

namespace LvlUp.Application.Hunters.UpdateDisplayNamePreference;

internal sealed class UpdateDisplayNamePreferenceCommandValidator
    : AbstractValidator<UpdateDisplayNamePreferenceCommand>
{
    public UpdateDisplayNamePreferenceCommandValidator()
    {
        RuleFor(command => command.HunterId).NotEmpty();
        RuleFor(command => command.Preference).IsInEnum();
    }
}
