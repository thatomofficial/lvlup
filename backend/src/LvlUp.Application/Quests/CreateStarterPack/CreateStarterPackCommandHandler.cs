using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Quests.CreateStarterPack;

internal sealed class CreateStarterPackCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    : ICommandHandler<CreateStarterPackCommand, CreateStarterPackResponse>
{
    private sealed record QuestTemplate(
        string Title,
        string Description,
        StatCategory Category,
        QuestType Type,
        QuestDifficulty? FixedDifficulty = null);

    private static readonly QuestTemplate[] Templates =
    [
        new("Workout session", "Strength training, calisthenics or gym - move with intent.", StatCategory.Strength, QuestType.Daily),
        new("Cardio session", "Run, cycle or brisk walk - get the heart rate up.", StatCategory.Stamina, QuestType.Daily),
        new("Stretch or mobility: 10 min", "Loosen up - morning or before bed.", StatCategory.Physique, QuestType.Daily),
        new("Drink 2L of water", "Hydration is a stat buff.", StatCategory.Physique, QuestType.Daily, QuestDifficulty.Easy),
        new("Skincare routine (AM + PM)", "Cleanse and moisturise, morning and night.", StatCategory.Looks, QuestType.Daily),
        new("Read the Bible", "Daily reading - start with the Gospel of John.", StatCategory.WellBeing, QuestType.Daily),
        new("Read a growth book", "Mindset, habits, confidence - keep turning pages.", StatCategory.Intelligence, QuestType.Daily),
        new("Personal coding practice", "Side projects, katas, learning - outside of work tasks.", StatCategory.Intelligence, QuestType.Daily),
        new("Start one conversation", "Greet someone, ask a question, make small talk. One counts.", StatCategory.Charisma, QuestType.Daily),
        new("Vocal training", "Breathing, articulation, reading aloud, recording yourself.", StatCategory.Charisma, QuestType.Daily),
        new("Finish your first growth book", "Complete one mindset book cover to cover.", StatCategory.Intelligence, QuestType.OneTime, QuestDifficulty.Elite),
        new("Finish the Gospel of John", "21 chapters - one Gospel, completed.", StatCategory.WellBeing, QuestType.OneTime, QuestDifficulty.Elite),
        new("Hold a 5-minute conversation", "Keep a conversation going for five minutes with someone new.", StatCategory.Charisma, QuestType.OneTime, QuestDifficulty.Elite),
        new("Ship a personal coding project", "Build something and put it out there.", StatCategory.Intelligence, QuestType.OneTime, QuestDifficulty.Elite),
    ];

    public async Task<Result<CreateStarterPackResponse>> HandleAsync(
        CreateStarterPackCommand command,
        CancellationToken cancellationToken)
    {
        Hunter? hunter = await context.Hunters
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.Id == command.HunterId, cancellationToken);

        if (hunter is null)
        {
            return Result.Failure<CreateStarterPackResponse>(HunterErrors.NotFound(command.HunterId));
        }

        List<string> existingTitles = await context.Quests
            .AsNoTracking()
            .Where(quest => quest.HunterId == command.HunterId)
            .Select(quest => quest.Title)
            .ToListAsync(cancellationToken);

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;
        int created = 0;

        foreach (QuestTemplate template in Templates)
        {
            if (existingTitles.Contains(template.Title, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            // Daily quests are calibrated to the hunter's current stat in that
            // category, so the board starts at their level - not above it.
            QuestDifficulty difficulty = template.FixedDifficulty
                ?? DifficultyForStat(hunter.GetStat(template.Category));

            context.Quests.Add(Quest.Create(
                command.HunterId,
                template.Title,
                template.Description,
                template.Category,
                difficulty,
                template.Type,
                utcNow));

            created++;
        }

        if (created > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return new CreateStarterPackResponse(created, Templates.Length - created);
    }

    private static QuestDifficulty DifficultyForStat(int statValue) => statValue switch
    {
        <= 8 => QuestDifficulty.Easy,
        <= 11 => QuestDifficulty.Medium,
        _ => QuestDifficulty.Hard,
    };
}
