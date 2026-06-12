using FluentValidation;

namespace LvlUp.Application.Quests.CreateQuest;

internal sealed class CreateQuestCommandValidator : AbstractValidator<CreateQuestCommand>
{
    public CreateQuestCommandValidator()
    {
        RuleFor(command => command.HunterId).NotEmpty();
        RuleFor(command => command.Title).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Description).MaximumLength(500);
        RuleFor(command => command.Category).IsInEnum();
        RuleFor(command => command.Difficulty).IsInEnum();
        RuleFor(command => command.Type).IsInEnum();
        RuleFor(command => command.Verification).IsInEnum();
    }
}
