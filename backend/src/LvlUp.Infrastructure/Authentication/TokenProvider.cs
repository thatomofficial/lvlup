using System.Security.Claims;
using System.Text;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Domain.Hunters;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace LvlUp.Infrastructure.Authentication;

internal sealed class TokenProvider(IOptions<JwtOptions> jwtOptions) : ITokenProvider
{
    public string Create(Hunter hunter)
    {
        JwtOptions options = jwtOptions.Value;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, hunter.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, hunter.Email),
                new Claim(JwtRegisteredClaimNames.Name, hunter.Name),
            ]),
            Expires = DateTime.UtcNow.AddMinutes(options.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = options.Issuer,
            Audience = options.Audience,
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }
}
