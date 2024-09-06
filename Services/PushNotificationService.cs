using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

using FleetPulse_BackEndDevelopment.Data;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly FleetPulseDbContext _context;
        private readonly ILogger<PushNotificationService> _logger;
        private readonly FirebaseMessaging _messaging;

        public PushNotificationService(FleetPulseDbContext context, ILogger<PushNotificationService> logger,
            FirebaseMessaging messaging)
        {
            _context = context;
            _logger = logger;
            InitializeFirebase();
            _messaging = messaging;
        }

        private void InitializeFirebase()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("Configuration/serviceAccountKey.json"),
                });
            }
        }

        public async Task SendNotificationAsync(string token, string title, string body,
            Dictionary<string, string> data)
        {
            var message = new Message
            {
                Token = token,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            // Assuming you have a configured FirebaseMessaging instance
            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }

        public async Task SaveNotificationAsync(FCMNotification notification)
        {
            try
            {
                notification.NotificationId = Guid.NewGuid().ToString();
                notification.Date = DateTime.UtcNow;
                notification.Time = DateTime.UtcNow.TimeOfDay;
                await _context.FCMNotifications.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving notification.");
            }
        }


        public async Task<List<FCMNotification>> GetAllNotificationsAsync()
        {
            try
            {
                return await _context.FCMNotifications.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications.");
                return new List<FCMNotification>();
            }
        }
        public async Task<List<FCMNotification>> GetUnreadNotificationsAsync()
        {
            try
            {
                return await _context.FCMNotifications.Where(n => !n.Status).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications.");
                return new List<FCMNotification>();
            }
        }

        public async Task MarkNotificationAsReadAsync(string notificationId)
        {
            try
            {
                var notification = await _context.FCMNotifications.FindAsync(notificationId);
                if (notification != null)
                {
                    notification.Status = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read.");
            }
        }

        public async Task MarkAllAsReadAsync()
        {
            try
            {
                var notifications = await _context.FCMNotifications.ToListAsync();
                notifications.ForEach(n => n.Status = true);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read.");
            }
        }

        public async Task DeleteNotificationAsync(string notificationId)
        {
            try
            {
                var notification = await _context.FCMNotifications.FindAsync(notificationId);
                if (notification != null)
                {
                    _context.FCMNotifications.Remove(notification);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification.");
            }
        }

        public async Task DeleteAllNotificationsAsync()
        {
            try
            {
                var notifications = await _context.FCMNotifications.ToListAsync();
                _context.FCMNotifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all notifications.");
            }
        }

        
        public async Task<bool> SendNotificationAsynctoAdmin(FCMNotificationDTO notification)
        {
            var emailExists = DoesEmailExist(notification.EmailAddress);
            if (!emailExists)
            {
                _logger.LogWarning("Email address not found: " + notification.EmailAddress);
                return false;
            }

            var username = GetUsernameByEmail(notification.EmailAddress);
            if (username == null)
            {
                _logger.LogWarning("Username not found for email address: " + notification.EmailAddress);
                return false;
            }

            var message = new Message()
            {
                Data = new Dictionary<string, string>()
                {
                    { "username", username },
                    { "jobTitle", notification.JobTitle },
                    { "title", notification.Title },
                    { "message", notification.Message },
                    { "emailAddress", notification.EmailAddress }
                },
                Notification = new Notification
                {
                    Title = notification.Title,
                    Body = notification.Message
                },
                Token = "e3LX8gFHuJ9t0tSET3dKGE:APA91bHqbnc5RYb7m3JgYJk348mJ95XkJyhDM0_lnjyNKtVrCBDgXmyrLwx-R5vc9gV9P6pnO2AZoMv4wy_OYqfYazVsCwayNQghGocBO3ee5uGOXV0jimBMhSWfUVq6ZruYZrMkw_qu" // replace with the actual admin device token
            };

            _logger.LogInformation($"Sending notification with data: {{Username: {notification.Username}, JobTitle: {notification.JobTitle}, Title: {notification.Title}, Message: {notification.Message}, EmailAddress: {notification.EmailAddress}}}");

            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Successfully sent message: " + response);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message.");
                return false;
            }
        }

        public bool DoesEmailExist(string email)
        {
            return _context.Users.Any(u => u.EmailAddress == email);
        }

        public string GetUsernameByEmail(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.EmailAddress == email);
            return user?.UserName;
        }
    }
}