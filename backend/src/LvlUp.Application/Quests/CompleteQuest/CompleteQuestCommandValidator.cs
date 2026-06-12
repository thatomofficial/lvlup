using FluentValidation;

namespace LvlUp.Application.Quests.CompleteQuest;

internal sealed class CompleteQuestCommandValidator : AbstractValidator<CompleteQuestCommand>
{
    public CompleteQuestCommandValidator()
    {
        RuleFor(command => command.HunterId).NotEmpty();
        RuleFor(command => command.QuestId).NotEmpty();
        RuleFor(command => command.Note).MaximumLength(280);
    }
}
