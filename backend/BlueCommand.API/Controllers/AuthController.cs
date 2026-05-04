using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BlueCommand.API.Services;
using BlueCommand.API.DTOs.Auth;
using BlueCommand.Application.Interfaces;
using BlueCommand.Domain.Entities;
using BlueCommand.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlueCommand.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly BlueCommandDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly JwtSettings _jwtSettings;
    private readonly IAuditService _audit;
    private readonly ICurrentUserService _currentUser;

    public AuthController(
        BlueCommandDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        JwtSettings jwtSettings,
        IAuditService audit,
        ICurrentUserService currentUser)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _jwtSettings = jwtSettings;
        _audit = audit;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var user = await _db.Utilizatori
            .Include(u => u.Rol)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Parola, user.ParolaHash))
            return Unauthorized(new { error = "Credențiale invalide" });

        if (!user.EsteActiv)
            return Unauthorized(new { error = "Cont dezactivat" });

        var now = DateTime.UtcNow;
        var expires = now.AddHours(_jwtSettings.ExpiryHours);

        var token = _jwt.CreateTokenForUser(user.Id, user.Username, user.Rol.Denumire, user.SectieId, expires);

        await _audit.LogActionAsync(user.Id, "LOGIN", $"User {user.Username} logged in", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);

        return Ok(new LoginResponseDto
        {
            Token = token,
            Utilizator = new UtilizatorAuthDto
            {
                Id = user.Id,
                Username = user.Username,
                Nume = user.Nume,
                Prenume = user.Prenume,
                Rol = user.Rol.Denumire,
                SectieId = user.SectieId
            }
        });
    }

    [HttpPost("logout")]
    [Authorize(Policy = "TotiUtilizatorii")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await _audit.LogActionAsync(_currentUser.UserId, "LOGOUT", $"User {_currentUser.Username} logged out", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }

    [HttpPut("schimba-parola")]
    [Authorize(Policy = "TotiUtilizatorii")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var user = await _db.Utilizatori.FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
        if (user is null)
            return Unauthorized();

        if (!_passwordHasher.Verify(request.ParolaActuala, user.ParolaHash))
            return BadRequest(new { error = "Parola curenta este incorecta" });

        user.ParolaHash = _passwordHasher.Hash(request.ParolaNoua);
        await _db.SaveChangesAsync(cancellationToken);

        await _audit.LogActionAsync(user.Id, "SCHIMBA_PAROLA", "User changed password", HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok();
    }
}
