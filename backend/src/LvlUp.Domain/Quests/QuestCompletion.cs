using LvlUp.Domain.Hunters;

namespace LvlUp.Domain.Quests;

public sealed class QuestCompletion
{
    private QuestCompletion()
    {
    }

    public Guid Id { get; private set; }

    public Guid QuestId { get; private set; }

    public Guid HunterId { get; private set; }

    public StatCategory Category { get; private set; }

    public int XpAwarded { get; private set; }

    public int StatAwarded { get; private set; }

    public DateTime CompletedAtUtc { get; private set; }

    public static QuestCompletion Create(Quest quest, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(quest);

        return new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = quest.Id,
            HunterId = quest.HunterId,
            Category = quest.Category,
            XpAwarded = quest.XpReward,
            StatAwarded = quest.StatReward,
            CompletedAtUtc = utcNow,
        };
    }
}
