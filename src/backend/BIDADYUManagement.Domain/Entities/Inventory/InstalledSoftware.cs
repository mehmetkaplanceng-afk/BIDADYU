using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.Inventory;

public class InstalledSoftware : BaseEntity
{
    public Guid ComputerId { get; set; }
    public string SoftwareName { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Version { get; set; }
    public string? InstallLocation { get; set; }
    public string? UninstallString { get; set; }
    public DateTime? InstalledDate { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeenAt { get; set; }
    public bool IsStillInstalled { get; set; } = true;

    // Matched to catalog (nullable - not all installed software may be in catalog)
    public Guid? MatchedSoftwareId { get; set; }
    public Guid? MatchedVersionId { get; set; }

    // Navigation
    public Computers.Computer Computer { get; set; } = null!;
    public Software.Software? MatchedSoftware { get; set; }
    public Software.SoftwareVersion? MatchedVersion { get; set; }
}
