using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;

namespace LvlUp.Domain.Quests;

public sealed class Quest : Entity
{
    private Quest()
    {
    }

    public Guid Id { get; private set; }

    public Guid HunterId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public StatCategory Category { get; private set; }

    public QuestDifficulty Difficulty { get; private set; }

    public QuestType Type { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? LastCompletedAtUtc { get; private set; }

    public int XpReward => Difficulty switch
    {
        QuestDifficulty.Easy => 10,
        QuestDifficulty.Medium => 25,
        QuestDifficulty.Hard => 50,
        QuestDifficulty.Elite => 100,
        _ => 0,
    };

    public int StatReward => Difficulty switch
    {
        QuestDifficulty.Easy => 1,
        QuestDifficulty.Medium => 2,
        QuestDifficulty.Hard => 3,
        QuestDifficulty.Elite => 5,
        _ => 0,
    };

    public static Quest Create(
        Guid hunterId,
        string title,
        string? description,
        StatCategory category,
        QuestDifficulty difficulty,
        QuestType type,
        DateTime utcNow) => new()
        {
            Id = Guid.NewGuid(),
            HunterId = hunterId,
            Title = title,
            Description = description,
            Category = category,
            Difficulty = difficulty,
            Type = type,
            CreatedAtUtc = utcNow,
        };

    public bool IsCompletedAt(DateTime utcNow) => Type switch
    {
        QuestType.OneTime => LastCompletedAtUtc is not null,
        QuestType.Daily => LastCompletedAtUtc is { } lastCompleted &&
                           DateOnly.FromDateTime(lastCompleted) == DateOnly.FromDateTime(utcNow),
        _ => false,
    };

    public Result Complete(DateTime utcNow)
    {
        if (IsCompletedAt(utcNow))
        {
            return Result.Failure(QuestErrors.AlreadyCompleted(Id));
        }

        LastCompletedAtUtc = utcNow;
        Raise(new QuestCompletedDomainEvent(Id, HunterId));

        return Result.Success();
    }
}
