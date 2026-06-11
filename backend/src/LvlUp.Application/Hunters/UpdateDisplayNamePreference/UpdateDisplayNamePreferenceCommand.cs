using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;

namespace LvlUp.Application.Hunters.UpdateDisplayNamePreference;

public sealed record UpdateDisplayNamePreferenceCommand(Guid HunterId, DisplayNamePreference Preference) : ICommand;
