namespace BIDADYUManagement.Domain.Entities.Base;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public abstract class AuditableEntity : BaseEntity
{
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

public abstract class SoftDeletableEntity : AuditableEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
