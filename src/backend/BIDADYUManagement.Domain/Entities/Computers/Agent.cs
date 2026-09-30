using BIDADYUManagement.Domain.Entities.Base;
using BIDADYUManagement.Domain.Enums;

namespace BIDADYUManagement.Domain.Entities.Computers;

public class Agent : BaseEntity
{
    public Guid ComputerId { get; set; }
    public string DeviceId { get; set; } = string.Empty;    // Permanent GUID generated on first run
    public string AgentVersion { get; set; } = string.Empty;
    public AgentRegistrationStatus RegistrationStatus { get; set; } = AgentRegistrationStatus.Pending;
    public string? ApiTokenHash { get; set; }               // SHA-256 hash of the actual token
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastHeartbeatAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RegistrationIp { get; set; }
    public string? LastKnownIp { get; set; }

    // Navigation
    public Computer Computer { get; set; } = null!;
    public ICollection<AgentHeartbeat> Heartbeats { get; set; } = new List<AgentHeartbeat>();
}
