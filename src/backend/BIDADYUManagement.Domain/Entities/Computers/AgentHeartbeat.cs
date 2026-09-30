using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.Computers;

public class AgentHeartbeat : BaseEntity
{
    public Guid AgentId { get; set; }
    public string? IpAddress { get; set; }
    public float? CpuUsagePercent { get; set; }
    public float? RamUsagePercent { get; set; }
    public float? DiskUsagePercent { get; set; }
    public long? RamTotalMb { get; set; }
    public long? RamFreeMb { get; set; }
    public string? AgentVersion { get; set; }
    public long? SystemUptimeSeconds { get; set; }

    // Navigation
    public Agent Agent { get; set; } = null!;
}
