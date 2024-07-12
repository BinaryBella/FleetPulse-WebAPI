namespace FleetPulse_BackEndDevelopment.DTOs
{
    public class FCMNotificationDTO
    {
        public string NotificationId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string JobTitle { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string EmailAddress { get; set; }
        public bool Status { get; set; }
    }
}
