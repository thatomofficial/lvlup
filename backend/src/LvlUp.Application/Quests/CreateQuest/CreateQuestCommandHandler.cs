using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;

namespace LvlUp.Application.Quests.CreateQuest;

internal sealed class CreateQuestCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    : ICommandHandler<CreateQuestCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateQuestCommand command, CancellationToken cancellationToken)
    {
        var quest = Quest.Create(
            command.HunterId,
            command.Title.Trim(),
            string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            command.Category,
            command.Difficulty,
            command.Type,
            timeProvider.GetUtcNow().UtcDateTime);

        quest.SetVerification(command.Verification);

        context.Quests.Add(quest);

        await context.SaveChangesAsync(cancellationToken);

        return quest.Id;
    }
}
