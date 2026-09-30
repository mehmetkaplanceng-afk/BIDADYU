using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.Compliance;

public class SoftwarePolicyItem : BaseEntity
{
    public Guid PolicyId { get; set; }
    public Guid SoftwareId { get; set; }
    public Guid? RequiredVersionId { get; set; }    // null = any version acceptable
    public bool IsRequired { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation
    public SoftwarePolicy Policy { get; set; } = null!;
    public Software.Software Software { get; set; } = null!;
    public Software.SoftwareVersion? RequiredVersion { get; set; }
}
