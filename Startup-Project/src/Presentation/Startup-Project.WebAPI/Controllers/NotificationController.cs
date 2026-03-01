using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Abstractions.Notification;
using Startup_Project.Application.Dtos.Notification;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/notifications")]
[ApiController]
[Authorize]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => Ok(await notificationService.GetMyNotificationsAsync(page, pageSize));

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
        => Ok(await notificationService.GetUnreadCountAsync());

    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
        => Ok(await notificationService.MarkAsReadAsync(notificationId));

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
        => Ok(await notificationService.MarkAllAsReadAsync());

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> Delete(Guid notificationId)
        => Ok(await notificationService.DeleteAsync(notificationId));

    [HttpPost("devices")]
    public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceDto dto)
        => Ok(await notificationService.RegisterDeviceAsync(dto));

    [HttpDelete("devices/{deviceToken}")]
    public async Task<IActionResult> UnregisterDevice(string deviceToken)
        => Ok(await notificationService.UnregisterDeviceAsync(deviceToken));
}
