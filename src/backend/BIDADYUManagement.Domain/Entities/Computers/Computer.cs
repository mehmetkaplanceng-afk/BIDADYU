using BIDADYUManagement.Domain.Entities.Base;
using BIDADYUManagement.Domain.Enums;

namespace BIDADYUManagement.Domain.Entities.Computers;

public class Computer : SoftDeletableEntity
{
    public string Hostname { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public string? OperatingSystem { get; set; }
    public string? OsVersion { get; set; }
    public string? OsArchitecture { get; set; }
    public string? CpuInfo { get; set; }
    public int? RamGb { get; set; }
    public string? DiskInfo { get; set; }
    public string? Location { get; set; }
    public string? Department { get; set; }
    public string? Notes { get; set; }
    public ComputerStatus Status { get; set; } = ComputerStatus.Offline;
    public DateTime? LastSeenAt { get; set; }
    public DateTime? FirstSeenAt { get; set; }

    // Navigation
    public ICollection<ComputerGroupMember> GroupMemberships { get; set; } = new List<ComputerGroupMember>();
    public Agent? Agent { get; set; }
    public ICollection<Inventory.InstalledSoftware> InstalledSoftwares { get; set; } = new List<Inventory.InstalledSoftware>();
    public ICollection<Deployments.DeploymentJobTarget> JobTargets { get; set; } = new List<Deployments.DeploymentJobTarget>();
}
