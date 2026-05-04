using System.Security.Claims;

namespace BlueCommand.Application.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(IEnumerable<Claim> claims, DateTime expiresUtc);
    string CreateTokenForUser(int userId, string username, string rolDenumire, int? sectieId, DateTime expiresUtc);
}
