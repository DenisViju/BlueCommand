using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BlueCommand.Application.Interfaces;

namespace BlueCommand.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var raw = User?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User?.FindFirstValue("userId");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? Username => User?.FindFirstValue("username") ?? User?.Identity?.Name;

    public string? Role => User?.FindFirstValue(ClaimTypes.Role) ?? User?.FindFirstValue("role") ?? User?.FindFirstValue("rol");

    public int? SectieId
    {
        get
        {
            var raw = User?.FindFirstValue("sectieId");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
