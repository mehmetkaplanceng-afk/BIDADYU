using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.Deployments;

public class DeploymentLog : BaseEntity
{
    public Guid JobTargetId { get; set; }
    public string Level { get; set; } = "Information";    // Information, Warning, Error
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }

    // Navigation
    public DeploymentJobTarget JobTarget { get; set; } = null!;
}
