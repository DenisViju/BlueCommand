using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlueCommand.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace BlueCommand.API.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(string secret, string issuer, string audience)
    {
        _secret = secret;
        _issuer = issuer;
        _audience = audience;
    }

    public string CreateToken(IEnumerable<Claim> claims, DateTime expiresUtc)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresUtc,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateTokenForUser(int userId, string username, string rolDenumire, int? sectieId, DateTime expiresUtc)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("userId", userId.ToString()),
            new("username", username),
            new(ClaimTypes.Role, rolDenumire),
            new("role", rolDenumire),
        };
        if (sectieId.HasValue)
            claims.Add(new Claim("sectieId", sectieId.Value.ToString()));

        return CreateToken(claims, expiresUtc);
    }
}
