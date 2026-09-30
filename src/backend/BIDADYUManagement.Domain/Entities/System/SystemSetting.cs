using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.System;

public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "General";
    public bool IsEncrypted { get; set; } = false;
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
}
