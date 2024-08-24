using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FleetPulse_BackEndDevelopment.Models
{
    public class Trip
    {
        [Key] public int TripId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public float StartMeterValue { get; set; }
        public float EndMeterValue { get; set; }
        public bool Status { get; set; }
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}