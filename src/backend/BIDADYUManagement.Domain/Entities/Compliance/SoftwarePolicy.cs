using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.Compliance;

public class SoftwarePolicy : AuditableEntity
{
    public Guid GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEnforced { get; set; } = false;   // Auto-deploy if non-compliant

    // Navigation
    public Computers.ComputerGroup Group { get; set; } = null!;
    public ICollection<SoftwarePolicyItem> Items { get; set; } = new List<SoftwarePolicyItem>();
}
