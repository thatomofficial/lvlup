using LvlUp.Domain.Hunters;

namespace LvlUp.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(Hunter hunter);
}
