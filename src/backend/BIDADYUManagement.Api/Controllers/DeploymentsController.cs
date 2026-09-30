using BIDADYUManagement.Domain.Entities.Deployments;
using BIDADYUManagement.Domain.Enums;
using BIDADYUManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIDADYUManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Roles.SuperAdmin + "," + Roles.SystemAdmin + "," + Roles.ITOperator)]
public class DeploymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DeploymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] CreateDeploymentJobRequest request)
    {
        // Generate Job Number
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.DeploymentJobs.CountAsync(j => j.CreatedAt.Date == DateTime.UtcNow.Date);
        var jobNumber = $"JOB-{today}-{(count + 1):D4}";

        // Eğer PackageId istekle gelmediyse SoftwareVersionId'ye ait ilk paketi otomatik bul
        var packageId = request.PackageId;
        if (!packageId.HasValue || packageId == Guid.Empty)
        {
            var activePackage = await _context.SoftwarePackages.FirstOrDefaultAsync(p => p.SoftwareVersionId == request.SoftwareVersionId);
            if (activePackage != null) packageId = activePackage.Id;
        }

        var job = new DeploymentJob
        {
            Id = Guid.NewGuid(),
            JobNumber = jobNumber,
            Action = request.Action,
            SoftwareId = request.SoftwareId,
            SoftwareVersionId = request.SoftwareVersionId,
            PackageId = packageId,
            Description = request.Description,
            Status = DeploymentStatus.Pending,
            ScheduledAt = request.ScheduledAt,
            TotalTargets = request.TargetComputerIds.Count
        };

        await _context.DeploymentJobs.AddAsync(job);

        var targets = request.TargetComputerIds.Select(cId => new DeploymentJobTarget
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            ComputerId = cId,
            Status = JobTargetStatus.Pending
        });

        await _context.DeploymentJobTargets.AddRangeAsync(targets);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, new { job.Id, job.JobNumber });
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs()
    {
        var jobs = await _context.DeploymentJobs
            .Include(j => j.Software)
            .Include(j => j.SoftwareVersion)
            .Include(j => j.Targets)
                .ThenInclude(t => t.Computer)
                    .ThenInclude(c => c.Agent)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new
            {
                j.Id,
                j.JobNumber,
                SoftwareName = j.Software.Name,
                Version = j.SoftwareVersion != null ? j.SoftwareVersion.Version : "1.0",
                Action = j.Action.ToString(),
                Status = j.Status.ToString(),
                j.TotalTargets,
                j.SuccessCount,
                j.FailedCount,
                j.CreatedAt,
                TargetComputers = j.Targets.Select(t => new {
                    t.ComputerId,
                    t.Computer.Hostname,
                    t.Computer.IpAddress,
                    Status = t.Status.ToString(),
                    Message = t.ErrorMessage,
                    t.StartedAt,
                    t.CompletedAt,
                    LastSeen = t.Computer.Agent != null ? t.Computer.Agent.LastHeartbeatAt : null,
                    IsApproved = t.Computer.Agent != null && t.Computer.Agent.RegistrationStatus == BIDADYUManagement.Domain.Enums.AgentRegistrationStatus.Approved
                })
            })
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobById(Guid id)
    {
        var job = await _context.DeploymentJobs
            .Include(j => j.Software)
            .Include(j => j.SoftwareVersion)
            .Include(j => j.Targets)
                .ThenInclude(t => t.Computer)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null) return NotFound();

        return Ok(new
        {
            job.Id,
            job.JobNumber,
            SoftwareName = job.Software.Name,
            Version = job.SoftwareVersion.Version,
            Action = job.Action.ToString(),
            Status = job.Status.ToString(),
            Targets = job.Targets.Select(t => new
            {
                t.Id,
                t.ComputerId,
                t.Computer.Hostname,
                Status = t.Status.ToString(),
                t.ErrorMessage
            })
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJob(Guid id)
    {
        var job = await _context.DeploymentJobs
            .Include(j => j.Targets)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null) return NotFound("Görev bulunamadı.");

        _context.DeploymentJobTargets.RemoveRange(job.Targets);
        _context.DeploymentJobs.Remove(job);

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("target-status")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateTargetStatus([FromBody] UpdateTargetStatusRequest request)
    {
        var target = await _context.DeploymentJobTargets
            .Include(t => t.Job)
            .FirstOrDefaultAsync(t => t.Id == request.TargetId);

        if (target == null) return NotFound("Target job not found.");

        if (Enum.TryParse<JobTargetStatus>(request.Status, true, out var parsedStatus))
        {
            target.Status = parsedStatus;
        }

        if (parsedStatus == JobTargetStatus.Installing)
        {
            target.StartedAt = DateTime.UtcNow;
        }
        else if (parsedStatus == JobTargetStatus.Success || parsedStatus == JobTargetStatus.Failed)
        {
            target.CompletedAt = DateTime.UtcNow;
            target.ErrorMessage = request.Message;
        }

        // Job genel istatistiklerini gÃ¼ncelle
        var jobTargets = await _context.DeploymentJobTargets.Where(t => t.JobId == target.JobId).ToListAsync();
        target.Job.SuccessCount = jobTargets.Count(t => t.Status == JobTargetStatus.Success);
        target.Job.FailedCount = jobTargets.Count(t => t.Status == JobTargetStatus.Failed);
        target.Job.PendingCount = jobTargets.Count(t => t.Status == JobTargetStatus.Pending || t.Status == JobTargetStatus.Installing);

        if (jobTargets.All(t => t.Status == JobTargetStatus.Success || t.Status == JobTargetStatus.Failed))
        {
            target.Job.Status = target.Job.FailedCount == 0 ? DeploymentStatus.Completed : DeploymentStatus.PartialSuccess;
            target.Job.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            target.Job.Status = DeploymentStatus.Running;
        }

        // Log kaydÄ± ekle
        var log = new DeploymentLog
        {
            Id = Guid.NewGuid(),
            JobTargetId = target.Id,
            Level = parsedStatus == JobTargetStatus.Failed ? "Error" : "Information",
            Message = request.Message ?? $"Durum gÃ¼ncellendi: {request.Status}",
            CreatedAt = DateTime.UtcNow
        };
        await _context.DeploymentLogs.AddAsync(log);

        await _context.SaveChangesAsync();
        return Ok();
    }
}

public class UpdateTargetStatusRequest
{
    public Guid TargetId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
}

public class CreateDeploymentJobRequest
{
    public DeploymentAction Action { get; set; }
    public Guid SoftwareId { get; set; }
    public Guid SoftwareVersionId { get; set; }
    public Guid? PackageId { get; set; }
    public List<Guid> TargetComputerIds { get; set; } = new List<Guid>();
    public DateTime? ScheduledAt { get; set; }
    public string? Description { get; set; }
}

