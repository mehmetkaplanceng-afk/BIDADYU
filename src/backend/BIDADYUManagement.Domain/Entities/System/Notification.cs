using BIDADYUManagement.Domain.Entities.Base;
using BIDADYUManagement.Domain.Enums;

namespace BIDADYUManagement.Domain.Entities.System;

public class Notification : BaseEntity
{
    public Guid? UserId { get; set; }               // null = all users
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed
}
