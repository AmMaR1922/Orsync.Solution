using System.Security.Claims;

namespace ApplicationLayer.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(string userId, string email, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
