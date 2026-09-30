using BIDADYUManagement.Application.Auth.Dtos;
using BIDADYUManagement.Application.Auth.Services;
using BIDADYUManagement.Domain.Entities.Auth;
using BIDADYUManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIDADYUManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;

    public AuthController(ApplicationDbContext context, ITokenService tokenService, IPasswordService passwordService)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { Message = "GeÃ§ersiz kullanÄ±cÄ± adÄ± veya ÅŸifre" });
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshTokenString = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15), // matches config
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles
            }
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var activeToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.ExpiresAt > DateTime.UtcNow && !rt.IsRevoked);

        if (activeToken == null || !activeToken.User.IsActive)
        {
            return Unauthorized(new { Message = "GeÃ§ersiz veya sÃ¼resi dolmuÅŸ token" });
        }

        // Revoke the old one
                activeToken.RevokedAt = DateTime.UtcNow;

        var roles = activeToken.User.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newAccessToken = _tokenService.GenerateAccessToken(activeToken.User, roles);
        var newRefreshTokenString = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = activeToken.UserId,
            Token = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _context.RefreshTokens.AddAsync(newRefreshToken);
        await _context.SaveChangesAsync();

        return Ok(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserDto
            {
                Id = activeToken.User.Id,
                Username = activeToken.User.Username,
                Email = activeToken.User.Email,
                FirstName = activeToken.User.FirstName,
                LastName = activeToken.User.LastName,
                Roles = roles
            }
        });
    }
}

