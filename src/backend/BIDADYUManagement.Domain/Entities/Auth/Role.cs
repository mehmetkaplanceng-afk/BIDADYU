using BIDADYUManagement.Domain.Entities.Base;

namespace BIDADYUManagement.Domain.Entities.Auth;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
