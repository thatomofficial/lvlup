using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Quests.DeleteQuest;

internal sealed class DeleteQuestCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteQuestCommand>
{
    public async Task<Result> HandleAsync(DeleteQuestCommand command, CancellationToken cancellationToken)
    {
        Quest? quest = await context.Quests
            .SingleOrDefaultAsync(
                q => q.Id == command.QuestId && q.HunterId == command.HunterId,
                cancellationToken);

        if (quest is null)
        {
            return Result.Failure(QuestErrors.NotFound(command.QuestId));
        }

        context.Quests.Remove(quest);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
