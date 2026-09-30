using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.System;

/// <summary>
/// Immutable audit log - INSERT ONLY. Never update or delete records.
/// </summary>
public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = "SYSTEM";
    public string Action { get; set; } = string.Empty;         // e.g. DEPLOY_SOFTWARE, CREATE_USER
    public string? EntityType { get; set; }                     // e.g. DeploymentJob, Software
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }                      // JSON
    public string? NewValues { get; set; }                      // JSON
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? FailureReason { get; set; }
}
