using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;

namespace LvlUp.Application.Hunters.SubmitAssessment;

public sealed record SubmitAssessmentCommand(
    Guid HunterId,
    IReadOnlyDictionary<StatCategory, int> Scores) : ICommand<AssessmentResponse>;
