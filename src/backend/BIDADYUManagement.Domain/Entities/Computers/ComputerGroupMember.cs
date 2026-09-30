namespace BIDADYUManagement.Domain.Entities.Computers;

public class ComputerGroupMember
{
    public Guid ComputerId { get; set; }
    public Guid GroupId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public Guid? AddedBy { get; set; }

    // Navigation
    public Computer Computer { get; set; } = null!;
    public ComputerGroup Group { get; set; } = null!;
}
