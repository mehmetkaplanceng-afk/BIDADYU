using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.Software;

public class SoftwareCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Software> Softwares { get; set; } = new List<Software>();
}
