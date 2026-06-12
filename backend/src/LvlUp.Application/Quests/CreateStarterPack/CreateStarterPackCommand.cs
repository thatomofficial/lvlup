using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Quests.CreateStarterPack;

public sealed record CreateStarterPackCommand(Guid HunterId) : ICommand<CreateStarterPackResponse>;

public sealed record CreateStarterPackResponse(int CreatedCount, int SkippedCount);
