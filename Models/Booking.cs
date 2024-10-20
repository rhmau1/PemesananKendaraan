using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace monitorKendaraan.Models
{
    public class Booking
    {
        [Key]
        public Guid booking_id { get; set; }

        [ForeignKey("user_id")] //user admin yang membuat pesanan
        public int user_id { get; set; }
        public User user { get; set; }
        [ForeignKey("vehicle_id")]
        public int vehicle_id { get; set; }
        public Vehicle vehicle { get; set; }
        [ForeignKey("driver_id")]
        public int driver_id { get; set; }
        public Driver driver { get; set; }
        [DataType(DataType.Date)]
        public DateTime start_booking { get; set; }
        [DataType(DataType.Date)]
        public DateTime end_booking { get; set; }
        [DataType(DataType.Date)]
        public DateTime created_at { get; set; }
        public int total_price { get; set; }
        [EnumDataType(typeof(status))]
        public status status { get; set; } = status.pending;
        public ICollection<Approval> Approvals { get; set; }
        public Booking()
        {
            this.created_at = DateTime.Now;
        }

    }
    public class BookingDTO
    {        
        public int driver_id { get; set; }
        public int vehicle_id { get; set; }        
        [DataType(DataType.Date)]
        public DateTime start_booking { get; set; }
        [DataType(DataType.Date)]
        public DateTime end_booking { get; set; }
        public List<ApprovalDTO> Approvers { get; set; } 

    }
    public class BookingResponseDTO
    {
        public Guid booking_id { get; set; }
        public int user_id { get; set; }
        public int vehicle_id { get; set; }
        public int driver_id { get; set; }
        public DateTime start_booking { get; set; }
        public DateTime end_booking { get; set; }
        public DateTime created_at { get; set; }
        public int? total_price { get; set; }
        public status status { get; set; }
    }
    public enum status
    {
        pending, 
        approved,
        rejected
    }
}
