namespace BIDADYUManagement.Domain.Entities.Auth;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedBy { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
