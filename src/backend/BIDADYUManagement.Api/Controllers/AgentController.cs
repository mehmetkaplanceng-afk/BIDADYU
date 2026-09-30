using BIDADYUManagement.Domain.Entities.Computers;
using BIDADYUManagement.Domain.Enums;
using BIDADYUManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BIDADYUManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AgentController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AgentController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] AgentRegistrationRequest request)
    {
        // For a real scenario, you'd validate a pre-shared key (PSK) here.
        
        // 1. Önce DeviceId ile, bulamazsa aynı Hostname (Bilgisayar Adı) ile ara (mükerrer PC engelleme)
        var agent = await _context.Agents.Include(a => a.Computer).FirstOrDefaultAsync(a => a.DeviceId == request.DeviceId);
        if (agent == null && !string.IsNullOrEmpty(request.Hostname))
        {
            agent = await _context.Agents.Include(a => a.Computer).FirstOrDefaultAsync(a => a.Computer.Hostname == request.Hostname);
        }
        
        string rawToken = Guid.NewGuid().ToString("N");
        string tokenHash = HashToken(rawToken);

        if (agent == null)
        {
            // First time registration
            var computer = new Computer
            {
                Id = Guid.NewGuid(),
                Hostname = request.Hostname,
                MacAddress = request.MacAddress,
                IpAddress = request.IpAddress,
                OsVersion = request.OsVersion,
                Status = ComputerStatus.Pending
            };
            await _context.Computers.AddAsync(computer);

            agent = new Agent
            {
                Id = Guid.NewGuid(),
                ComputerId = computer.Id,
                DeviceId = request.DeviceId,
                AgentVersion = request.AgentVersion,
                RegistrationStatus = AgentRegistrationStatus.Pending,
                ApiTokenHash = tokenHash,
                RegistrationIp = request.IpAddress,
                LastKnownIp = request.IpAddress
            };
            await _context.Agents.AddAsync(agent);
        }
        else
        {
            // Re-registration
            agent.ApiTokenHash = tokenHash;
            agent.LastKnownIp = request.IpAddress;
            agent.AgentVersion = request.AgentVersion;
            
            agent.Computer.Hostname = request.Hostname;
            agent.Computer.MacAddress = request.MacAddress;
            agent.Computer.IpAddress = request.IpAddress;
            agent.Computer.OsVersion = request.OsVersion;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Status = agent.RegistrationStatus.ToString(),
            Token = rawToken // Sent only once!
        });
    }

    [HttpPost("heartbeat")]
    [AllowAnonymous] // Will use custom token auth in a real scenario (via middleware)
    public async Task<IActionResult> Heartbeat([FromHeader(Name = "X-Agent-Token")] string token, [FromBody] AgentHeartbeatRequest request)
    {
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        string tokenHash = HashToken(token);
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.ApiTokenHash == tokenHash && a.IsActive);

        if (agent == null || agent.RegistrationStatus != AgentRegistrationStatus.Approved)
        {
            return Unauthorized(new { Message = "Agent not approved or invalid token" });
        }

        agent.LastHeartbeatAt = DateTime.UtcNow;
        agent.LastKnownIp = request.IpAddress;
        if (agent.Computer != null)
        {
            agent.Computer.Status = Domain.Enums.ComputerStatus.Online;
        }

        var heartbeat = new AgentHeartbeat
        {
            Id = Guid.NewGuid(),
            AgentId = agent.Id,
            IpAddress = request.IpAddress,
            CpuUsagePercent = request.CpuUsagePercent,
            RamUsagePercent = request.RamUsagePercent,
            DiskUsagePercent = request.DiskUsagePercent,
            RamTotalMb = request.RamTotalMb,
            RamFreeMb = request.RamFreeMb,
            SystemUptimeSeconds = request.SystemUptimeSeconds,
            AgentVersion = agent.AgentVersion
        };

        await _context.AgentHeartbeats.AddAsync(heartbeat);

        // Here we could also check if there are pending jobs for this agent and return them.
        // Pending veya Queued olan işleri getir
        var pendingJobs = await _context.DeploymentJobTargets
            .Include(t => t.Job)
                .ThenInclude(j => j.SoftwareVersion)
                    .ThenInclude(v => v.Packages)
            .Where(t => t.ComputerId == agent.ComputerId && (t.Status == JobTargetStatus.Pending || t.Status == JobTargetStatus.Queued))
            .Select(t => new
            {
                JobId = t.Job.Id,
                TargetId = t.Id,
                Action = t.Job.Action.ToString(),
                SoftwareId = t.Job.SoftwareId,
                Version = t.Job.SoftwareVersion != null ? t.Job.SoftwareVersion.Version : "1.0",
                PackageId = t.Job.PackageId ?? (t.Job.SoftwareVersion != null && t.Job.SoftwareVersion.Packages.Any() ? t.Job.SoftwareVersion.Packages.FirstOrDefault()!.Id : (Guid?)null),
                FileName = t.Job.Package != null ? t.Job.Package.FileName : (t.Job.SoftwareVersion != null && t.Job.SoftwareVersion.Packages.Any() ? t.Job.SoftwareVersion.Packages.FirstOrDefault()!.FileName : "setup.msi"),
                SilentArgs = t.Job.Package != null ? t.Job.Package.SilentInstallArgs : (t.Job.SoftwareVersion != null && t.Job.SoftwareVersion.Packages.Any() ? t.Job.SoftwareVersion.Packages.FirstOrDefault()!.SilentInstallArgs : "/qn /norestart"),
                InstallerType = t.Job.Package != null ? t.Job.Package.InstallerType.ToString() : "Msi"
            })
            .ToListAsync();

        await _context.SaveChangesAsync();

        return Ok(new { Jobs = pendingJobs });
    }

    [HttpPost("inventory")]
    [AllowAnonymous]
    public async Task<IActionResult> ReportInventory([FromHeader(Name = "X-Agent-Token")] string token, [FromBody] AgentInventoryRequest request)
    {
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        string tokenHash = HashToken(token);
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.ApiTokenHash == tokenHash && a.IsActive);

        if (agent == null || agent.RegistrationStatus != AgentRegistrationStatus.Approved)
        {
            return Unauthorized();
        }

        // Mark all existing as not seen (soft delete logic for inventory)
        var existingSoftware = await _context.InstalledSoftwares
            .Where(s => s.ComputerId == agent.ComputerId && s.IsStillInstalled)
            .ToListAsync();

        foreach (var software in existingSoftware)
        {
            software.IsStillInstalled = false;
        }

        // Add or update from report
        foreach (var item in request.SoftwareList)
        {
            var match = existingSoftware.FirstOrDefault(s => s.SoftwareName == item.Name && s.Version == item.Version);
            
            if (match != null)
            {
                match.IsStillInstalled = true;
                match.LastSeenAt = DateTime.UtcNow;
            }
            else
            {
                var newSoftware = new BIDADYUManagement.Domain.Entities.Inventory.InstalledSoftware
                {
                    Id = Guid.NewGuid(),
                    ComputerId = agent.ComputerId,
                    SoftwareName = item.Name,
                    Publisher = item.Publisher,
                    Version = item.Version,
                    InstallLocation = item.InstallLocation,
                    UninstallString = item.UninstallString,
                    IsStillInstalled = true,
                    DetectedAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                };
                await _context.InstalledSoftwares.AddAsync(newSoftware);
            }
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("ticket")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateTicket([FromHeader(Name = "X-Agent-Token")] string token, [FromBody] CreateAgentTicketRequest request)
    {
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        string tokenHash = HashToken(token);
        var agent = await _context.Agents.Include(a => a.Computer).FirstOrDefaultAsync(a => a.ApiTokenHash == tokenHash && a.IsActive);

        if (agent == null) return Unauthorized("Geçersiz Agent simgesi.");

        var notification = new BIDADYUManagement.Domain.Entities.System.Notification
        {
            Id = Guid.NewGuid(),
            Title = request.TicketType == "HardwareIssue" ? $"🛠️ Arıza Kaydı: {agent.Computer.Hostname}" : $"📦 Eksik Program Talebi: {agent.Computer.Hostname}",
            Message = request.Message,
            Type = request.TicketType == "HardwareIssue" ? BIDADYUManagement.Domain.Enums.NotificationType.Warning : BIDADYUManagement.Domain.Enums.NotificationType.Info,
            EntityType = "Computer",
            EntityId = agent.ComputerId.ToString(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Talebiniz başarıyla yönetime iletildi!", NotificationId = notification.Id });
    }

    [HttpGet("ticket-status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLastTicketStatus([FromHeader(Name = "X-Agent-Token")] string token)
    {
        if (string.IsNullOrEmpty(token)) return Unauthorized();

        string tokenHash = HashToken(token);
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.ApiTokenHash == tokenHash && a.IsActive);

        if (agent == null) return Unauthorized();

        var lastTicket = await _context.Notifications
            .Where(n => n.EntityType == "Computer" && n.EntityId == agent.ComputerId.ToString())
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastTicket == null)
        {
            return Ok(new { HasTicket = false });
        }

        return Ok(new
        {
            HasTicket = true,
            Title = lastTicket.Title,
            Message = lastTicket.Message,
            Status = lastTicket.Status, // Pending, InProgress, Completed
            CreatedAt = lastTicket.CreatedAt
        });
    }

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}

public class CreateAgentTicketRequest
{
    public string TicketType { get; set; } = "HardwareIssue"; // HardwareIssue, SoftwareRequest
    public string Message { get; set; } = string.Empty;
}

public class AgentRegistrationRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string? MacAddress { get; set; }
    public string? IpAddress { get; set; }
    public string? OsVersion { get; set; }
    public string AgentVersion { get; set; } = string.Empty;
}

public class AgentHeartbeatRequest
{
    public string? IpAddress { get; set; }
    public float CpuUsagePercent { get; set; }
    public float RamUsagePercent { get; set; }
public class AgentInventoryRequest
{
    public List<SoftwareInventoryItem> SoftwareList { get; set; } = new List<SoftwareInventoryItem>();
}

public class SoftwareInventoryItem
{
    public string Name { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Version { get; set; }
    public string? InstallLocation { get; set; }
    public string? UninstallString { get; set; }
}
    public float DiskUsagePercent { get; set; }
    public long RamTotalMb { get; set; }
    public long RamFreeMb { get; set; }
    public long SystemUptimeSeconds { get; set; }
}

public class AgentInventoryRequest
{
    public List<SoftwareInventoryItem> SoftwareList { get; set; } = new List<SoftwareInventoryItem>();
}

public class SoftwareInventoryItem
{
    public string Name { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Version { get; set; }
    public string? InstallLocation { get; set; }
    public string? UninstallString { get; set; }
}
