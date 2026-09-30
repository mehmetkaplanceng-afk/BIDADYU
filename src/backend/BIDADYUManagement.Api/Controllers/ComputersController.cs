using BIDADYUManagement.Domain.Entities.Computers;
using BIDADYUManagement.Domain.Enums;
using BIDADYUManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIDADYUManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ComputersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ComputersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var threshold = DateTime.UtcNow.AddSeconds(-35);

        var computers = await _context.Computers
            .Include(c => c.Agent)
            .Include(c => c.GroupMemberships)
                .ThenInclude(gm => gm.Group)
            .Select(c => new
            {
                c.Id,
                c.Hostname,
                c.MacAddress,
                c.IpAddress,
                c.OsVersion,
                // Eğer Agent varsa ve Onaylı ise, Son Görülme (LastHeartbeatAt) 35 saniyeden eskiyse Çevrimdışı yap
                Status = (c.Agent != null && c.Agent.RegistrationStatus == BIDADYUManagement.Domain.Enums.AgentRegistrationStatus.Approved)
                    ? (c.Agent.LastHeartbeatAt.HasValue && c.Agent.LastHeartbeatAt.Value >= threshold ? "Online" : "Offline")
                    : c.Status.ToString(),
                AgentId = c.Agent != null ? c.Agent.Id : (Guid?)null,
                IsApproved = c.Agent != null && c.Agent.RegistrationStatus == BIDADYUManagement.Domain.Enums.AgentRegistrationStatus.Approved,
                AgentVersion = c.Agent != null ? c.Agent.AgentVersion : null,
                LastSeen = c.Agent != null ? c.Agent.LastHeartbeatAt : null,
                Groups = c.GroupMemberships.Select(gm => new { gm.Group.Id, gm.Group.Name })
            })
            .ToListAsync();

        return Ok(computers);
    }

    [HttpPost("approve-agent/{id}")]
    public async Task<IActionResult> ApproveAgent(Guid id)
    {
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.ComputerId == id);
        if (agent == null) return NotFound("Agent bulunamadı.");

        agent.RegistrationStatus = AgentRegistrationStatus.Approved;
        agent.ApprovedAt = DateTime.UtcNow;
        
        var computer = await _context.Computers.FindAsync(id);
        if (computer != null) computer.Status = ComputerStatus.Online;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Agent onaylandı." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComputer(Guid id)
    {
        var computer = await _context.Computers.FindAsync(id);
        if (computer == null) return NotFound();

        _context.Computers.Remove(computer);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Bilgisayar silindi." });
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDeleteComputers([FromBody] List<Guid> ids)
    {
        if (ids == null || ids.Count == 0) return BadRequest("Lütfen silinecek bilgisayarları seçin.");

        var computers = await _context.Computers.Where(c => ids.Contains(c.Id)).ToListAsync();
        _context.Computers.RemoveRange(computers);
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"{computers.Count} adet bilgisayar sistemden silindi." });
    }

    [HttpGet("{id}/software")]
    public async Task<IActionResult> GetInstalledSoftware(Guid id)
    {
        var softwareList = await _context.InstalledSoftwares
            .Where(s => s.ComputerId == id && s.IsStillInstalled)
            .OrderBy(s => s.SoftwareName)
            .Select(s => new
            {
                s.Id,
                s.SoftwareName,
                s.Publisher,
                s.Version,
                s.InstallLocation,
                s.UninstallString,
                s.DetectedAt,
                s.LastSeenAt
            })
            .ToListAsync();

        return Ok(softwareList);
    }
}
