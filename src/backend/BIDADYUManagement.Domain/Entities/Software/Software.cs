using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.Software;

public class Software : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string Platform { get; set; } = "Windows";
    public string? Description { get; set; }
    public string? HomepageUrl { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public SoftwareCategory? Category { get; set; }
    public ICollection<SoftwareVersion> Versions { get; set; } = new List<SoftwareVersion>();
    public ICollection<Inventory.InstalledSoftware> InstalledInstances { get; set; } = new List<Inventory.InstalledSoftware>();
    public ICollection<Compliance.SoftwarePolicyItem> PolicyItems { get; set; } = new List<Compliance.SoftwarePolicyItem>();
}
