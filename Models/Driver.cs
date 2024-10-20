﻿using System.ComponentModel.DataAnnotations;

namespace monitorKendaraan.Models
{
    public class Driver
    {
        [Key]
        public int driver_id { get; set; }
        public string name { get; set; }
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone number must be between 10 and 15 digits and contain only numbers")]
        public string phone_number { get; set; }
        public bool is_available { get; set; } = true;
        [DataType(DataType.Date)]
        public DateTime created_at { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public Driver()
        {
            this.created_at = DateTime.Now;
        }

    }
    public class DriverDTO
    {
        public string name { get; set; }
        public string phone_number { get; set; }
    }
}
