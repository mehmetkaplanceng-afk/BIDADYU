using BIDADYUManagement.Domain.Entities.Auth;
using BIDADYUManagement.Domain.Enums;
using BIDADYUManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BIDADYUManagement.Application.Auth.Services;

namespace BIDADYUManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.SuperAdmin + "," + Roles.SystemAdmin)]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;

    public UsersController(ApplicationDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.IsActive,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
        {
            return BadRequest(new { Message = "Kullanıcı adı veya e-posta zaten kullanımda." });
        }

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = _passwordService.HashPassword(request.Password),
            IsActive = true
        };

        await _context.Users.AddAsync(user);

        var roles = await _context.Roles.Where(r => request.RoleIds.Contains(r.Id)).ToListAsync();
        foreach (var role in roles)
        {
            await _context.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = role.Id });
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new { user.Id, user.Username });
    }

    // Additional endpoints for update/delete would go here...
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<Guid> RoleIds { get; set; } = new List<Guid>();
}
