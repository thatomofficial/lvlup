using System.Globalization;
using System.Security.Cryptography;
using LvlUp.Application.Abstractions.Authentication;

namespace LvlUp.Infrastructure.Authentication;

internal sealed class OtpGenerator : IOtpGenerator
{
    public string Generate()
    {
        int value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6", CultureInfo.InvariantCulture);
    }
}
