using BIDADYUManagement.Domain.Entities.Software;
using BIDADYUManagement.Domain.Enums;
using BIDADYUManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIDADYUManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SoftwareController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SoftwareController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var software = await _context.Softwares
            .Include(s => s.Category)
            .Include(s => s.Versions)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Publisher,
                s.Description,
                Category = s.Category != null ? s.Category.Name : "Genel",
                Versions = s.Versions.Select(v => new { v.Id, v.Version, v.IsCurrent })
            })
            .ToListAsync();

        return Ok(software);
    }

    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.SystemAdmin)]
    public async Task<IActionResult> Create([FromBody] CreateSoftwareRequest request)
    {
        var software = new Software
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Publisher = request.Publisher,
            Description = request.Description
        };

        await _context.Softwares.AddAsync(software);
        await _context.SaveChangesAsync();

        return Ok(software);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.SystemAdmin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var software = await _context.Softwares.FindAsync(id);
        if (software == null) return NotFound();

        _context.Softwares.Remove(software);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Yazılım kataloğundan silindi." });
    }

    [HttpPost("{softwareId}/versions")]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.SystemAdmin)]
    public async Task<IActionResult> AddVersion(Guid softwareId, [FromBody] AddVersionRequest request)
    {
        var software = await _context.Softwares.Include(s => s.Versions).FirstOrDefaultAsync(s => s.Id == softwareId);
        if (software == null) return NotFound("Yazılım bulunamadı.");

        foreach (var v in software.Versions) v.IsCurrent = false;

        var version = new SoftwareVersion
        {
            Id = Guid.NewGuid(),
            SoftwareId = softwareId,
            Version = request.Version,
            ReleaseDate = DateTime.UtcNow,
            IsCurrent = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.SoftwareVersions.AddAsync(version);
        await _context.SaveChangesAsync();

        return Ok(version);
    }
}

public class CreateSoftwareRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Description { get; set; }
}

public class AddVersionRequest
{
    public string Version { get; set; } = string.Empty;
}
