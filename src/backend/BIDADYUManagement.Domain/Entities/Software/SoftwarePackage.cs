using BIDADYUManagement.Domain.Entities.Base;
using BIDADYUManagement.Domain.Enums;

namespace BIDADYUManagement.Domain.Entities.Software;

public class SoftwarePackage : BaseEntity
{
    public Guid SoftwareVersionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public InstallerType InstallerType { get; set; } = InstallerType.Exe;
    public string? SilentInstallArgs { get; set; }
    public string? SilentUninstallArgs { get; set; }
    public int InstallTimeoutMinutes { get; set; } = 30;
    public string? Sha256Hash { get; set; }
    public long FileSizeBytes { get; set; }
    public Architecture Architecture { get; set; } = Architecture.X64;

    // Navigation
    public SoftwareVersion SoftwareVersion { get; set; } = null!;
}
