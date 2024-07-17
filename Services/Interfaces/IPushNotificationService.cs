using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FleetPulse_BackEndDevelopment.Services.Interfaces
{
    public interface IPushNotificationService
    {
        Task SaveNotificationAsync(FCMNotification notification);
        Task<List<FCMNotification>> GetAllNotificationsAsync();
        Task<List<FCMNotification>> GetUnreadNotificationsAsync();
        Task MarkNotificationAsReadAsync(string notificationId);
        Task MarkAllAsReadAsync();
        Task DeleteNotificationAsync(string notificationId);
        Task DeleteAllNotificationsAsync();
        Task<bool> SendNotificationAsynctoAdmin(FCMNotificationDTO notification);
        Task SendNotificationAsync(string token, string title, string message, Dictionary<string, string> dataPayload);
        bool DoesEmailExist(string email); // Check if email exists in the system
        string GetUsernameByEmail(string email); // Retrieve username by email
    }
}