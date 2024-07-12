using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;

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
        bool DoesEmailExist(string email); // Add this method to check email existence
        string GetUsernameByEmail(string email); // New method to get username by email

    }
}