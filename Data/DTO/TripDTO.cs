﻿using FleetPulse_BackEndDevelopment.Models;
using System.ComponentModel.DataAnnotations;

namespace FleetPulse_BackEndDevelopment.Data.DTO
{
    public class TripDTO
    {
        public int TripId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        public float StartMeterValue { get; set; }
        public float EndMeterValue { get; set; }
        public string VehicleRegistrationNo { get; set; }
        public string NIC { get; set; }
        public bool Status { get; set; }
    }
}
