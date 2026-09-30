using BIDADYUManagement.Domain.Entities.Base;
using BIDADYUManagement.Domain.Enums;

namespace BIDADYUManagement.Domain.Entities.Deployments;

public class DeploymentJobTarget : BaseEntity
{
    public Guid JobId { get; set; }
    public Guid ComputerId { get; set; }
    public JobTargetStatus Status { get; set; } = JobTargetStatus.Pending;
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; } = 0;
    public int? ExitCode { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? AgentPickedUpAt { get; set; }

    // Navigation
    public DeploymentJob Job { get; set; } = null!;
    public Computers.Computer Computer { get; set; } = null!;
    public ICollection<DeploymentLog> Logs { get; set; } = new List<DeploymentLog>();
}
