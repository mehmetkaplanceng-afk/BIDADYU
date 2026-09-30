using BIDADYUManagement.Domain.Entities.Base;
using BIDADYUManagement.Domain.Enums;

namespace BIDADYUManagement.Domain.Entities.Deployments;

public class DeploymentJob : AuditableEntity
{
    public string JobNumber { get; set; } = string.Empty;     // e.g. JOB-000124
    public DeploymentAction Action { get; set; } = DeploymentAction.Install;
    public Guid SoftwareId { get; set; }
    public Guid SoftwareVersionId { get; set; }
    public Guid? PackageId { get; set; }
    public string? Description { get; set; }
    public DeploymentStatus Status { get; set; } = DeploymentStatus.Pending;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Stats (denormalized for performance)
    public int TotalTargets { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public int PendingCount { get; set; }

    // Navigation
    public Software.Software Software { get; set; } = null!;
    public Software.SoftwareVersion SoftwareVersion { get; set; } = null!;
    public Software.SoftwarePackage? Package { get; set; }
    public ICollection<DeploymentJobTarget> Targets { get; set; } = new List<DeploymentJobTarget>();
}
