using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetPulse_BackEndDevelopment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IPushNotificationService _pushNotificationService;

        public NotificationController(IPushNotificationService pushNotificationService)
        {
            _pushNotificationService = pushNotificationService;
        }
        
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FCMNotification>>> GetNotifications()
        {
            // Simulate getting notifications in memory
            var notifications = await _pushNotificationService.GetAllNotificationsAsync();
            return Ok(notifications);
        }
        
        [AllowAnonymous]
        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<FCMNotification>>> GetUnreadNotifications()
        {
            // Simulate getting unread notifications in memory
            var notifications = await _pushNotificationService.GetUnreadNotificationsAsync();
            return Ok(notifications);
        }
        
        [AllowAnonymous]
        [HttpPost("mark-as-read/{id}")]
        public async Task<IActionResult> MarkNotificationAsRead(string id)
        {
            // Simulate marking a notification as read in memory
            await _pushNotificationService.MarkNotificationAsReadAsync(id);
            return Ok(new { Status = "Success", Message = "Notification marked as read" });
        }
        
        [AllowAnonymous]
        [HttpPost("markAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            // Simulate marking all notifications as read in memory
            await _pushNotificationService.MarkAllAsReadAsync();
            return Ok(new { Status = "Success", Message = "All notifications marked as read" });
        }
        
        [AllowAnonymous]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            // Simulate deleting a notification in memory
            await _pushNotificationService.DeleteNotificationAsync(id);
            return Ok(new { Status = "Success", Message = "Notification deleted successfully" });
        }
        [AllowAnonymous]
        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            // Simulate deleting all notifications in memory
            await _pushNotificationService.DeleteAllNotificationsAsync();
            return Ok(new { Status = "Success", Message = "All notifications deleted" });
        }
        
        [HttpPost("send-reset-password-notification")]
        public async Task<IActionResult> SendResetPasswordNotification([FromBody] FCMNotificationDTO notificationDto)
        {
            if (notificationDto == null || string.IsNullOrEmpty(notificationDto.EmailAddress))
            {
                return BadRequest("Invalid notification data.");
            }

            var notification = new FCMNotificationDTO
            {
                NotificationId = Guid.NewGuid().ToString(),
                Username = notificationDto.Username,
                JobTitle = notificationDto.JobTitle,
                Title = "Password Reset Request",
                Message = "User has requested a password reset.",
                Date = DateTime.UtcNow,
                Time = DateTime.UtcNow.TimeOfDay,
                EmailAddress = notificationDto.EmailAddress,
                Status = false
            };

            var result = await _pushNotificationService.SendNotificationAsynctoAdmin(notification);

            if (result)
            {
                return Ok("Notification sent successfully.");
            }

            return StatusCode(500, "Error sending notification.");
        }
        
    }
}