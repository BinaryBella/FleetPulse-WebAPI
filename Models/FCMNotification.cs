using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetPulse_BackEndDevelopment.Models
{
    public class FCMNotification
    {
        [Key]
        public string NotificationId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }
        
        public bool Status { get; set; }
    }
}