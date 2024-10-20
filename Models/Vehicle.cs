using System.ComponentModel.DataAnnotations;

namespace monitorKendaraan.Models
{
    public class Vehicle
    {
        [Key]
        public int vehicle_id { get; set; }
        public type type { get; set; }
        public ownership ownership { get; set; }
        public string plate_number { get; set; }
        [DataType(DataType.Date)]
        public DateTime created_at { get; set; }
        public bool is_available { get; set; } = true;
        public int rental_price_per_day { get; set; } = 0;
        public ICollection<Booking> Bookings { get; set; }
        public Vehicle()
        {
            this.created_at = DateTime.Now;
        }
    }
    public enum type
    {
        passenger,
        freight
    }
    public enum ownership
    {
        company,
        rent
    }
    public class VehicleDTO
    {
        public type type { get; set; }
        public string plate_number { get; set; }
        public int rental_price_per_day { get; set; }
        public ownership ownership { get; set; }

    }
}
