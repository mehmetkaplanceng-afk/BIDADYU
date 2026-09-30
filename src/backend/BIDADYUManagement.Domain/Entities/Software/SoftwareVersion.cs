using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.Software;

public class SoftwareVersion : AuditableEntity
{
    public Guid SoftwareId { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime? ReleaseDate { get; set; }
    public string? ReleaseNotes { get; set; }
    public bool IsCurrent { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Software Software { get; set; } = null!;
    public ICollection<SoftwarePackage> Packages { get; set; } = new List<SoftwarePackage>();
    public ICollection<Inventory.InstalledSoftware> InstalledInstances { get; set; } = new List<Inventory.InstalledSoftware>();
    public ICollection<Deployments.DeploymentJob> DeploymentJobs { get; set; } = new List<Deployments.DeploymentJob>();
}
