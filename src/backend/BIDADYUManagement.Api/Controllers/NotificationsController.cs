using BIDADYUManagement.Domain.Entities.System;
using BIDADYUManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BIDADYUManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var notifications = await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                Type = n.Type.ToString(),
                n.EntityType,
                n.EntityId,
                n.IsRead,
                n.Status,
                n.CreatedAt
            })
            .ToListAsync();

        int unreadCount = await _context.Notifications.CountAsync(n => !n.IsRead);

        return Ok(new
        {
            Notifications = notifications,
            UnreadCount = unreadCount
        });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        if (notification.Status == "Pending")
        {
            notification.Status = "InProgress"; // Admin bildirime tıkladığında duruma 'Şu an ilgileniliyor' atar
        }

        await _context.SaveChangesAsync();
        return Ok(new { notification.Status });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateNotificationStatusRequest request)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        notification.Status = request.Status;
        notification.IsRead = true;
        if (!notification.ReadAt.HasValue) notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { notification.Status });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null) return NotFound();

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

public class UpdateNotificationStatusRequest
{
    public string Status { get; set; } = "InProgress"; // Pending, InProgress, Completed
}
