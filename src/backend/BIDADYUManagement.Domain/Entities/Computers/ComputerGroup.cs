using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.Computers;

public class ComputerGroup : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ComputerGroupMember> Members { get; set; } = new List<ComputerGroupMember>();
    public ICollection<Compliance.SoftwarePolicy> Policies { get; set; } = new List<Compliance.SoftwarePolicy>();
}
