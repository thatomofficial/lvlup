using FluentValidation;
using LvlUp.Domain.Hunters;

namespace LvlUp.Application.Hunters.SubmitAssessment;

internal sealed class SubmitAssessmentCommandValidator : AbstractValidator<SubmitAssessmentCommand>
{
    public SubmitAssessmentCommandValidator()
    {
        RuleFor(command => command.HunterId).NotEmpty();

        RuleFor(command => command.Scores)
            .NotEmpty()
            .Must(scores => Enum.GetValues<StatCategory>().All(scores.ContainsKey))
            .WithMessage("Every stat category must be scored.");

        RuleForEach(command => command.Scores)
            .Must(score => score.Value is >= Hunter.MinAssessmentScore and <= Hunter.MaxAssessmentScore)
            .WithMessage($"Scores must be between {Hunter.MinAssessmentScore} and {Hunter.MaxAssessmentScore}.");
    }
}
