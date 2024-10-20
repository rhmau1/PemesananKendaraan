using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace monitorKendaraan.Models
{
    public class Approval
    {
        [Key]
        public int approval_id { get; set; }
        [ForeignKey("booking_id")]
        public Guid booking_id { get; set; }
        public Booking booking { get; set; }
        [ForeignKey("user_id")] //user approver yang menyetujui pesanan
        public int user_id { get; set; }
        public User user { get; set; }
        public int approval_level { get; set; }
        public bool is_approved { get; set; }
        [DataType(DataType.Date)]
        public DateTime approved_at { get; set; }
    }
    public class ApprovalDTO
    {
        //public Guid booking_id { get; set; }
        public int user_id { get; set; }
        public int approval_level { get; set; }
    }
    public class ApprovalResponseDTO
    {
        public int approval_id { get; set; }
        public Guid booking_id { get; set; }        
        public int user_id { get; set; }
        public int approval_level { get; set; }
        public bool is_approved { get; set; }
        [DataType(DataType.Date)]
        public DateTime approved_at { get; set; }
    }
}
