using System.ComponentModel.DataAnnotations;

namespace monitorKendaraan.Models
{
    public class User
    {
        [Key]
        public int user_id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        //[DataType(DataType.Date)]
        public DateTime created_at { get; set; }
        [EnumDataType(typeof(role))]
        public role user_role { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Approval>? Approvals { get; set; }

        public User()
        {
            this.created_at = DateTime.Now;
        }

    }

    public class UserDto
    {
        public string name { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        [EnumDataType(typeof(role))]
        public role user_role { get; set; }
    }

    public class LoginDTO
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public enum role
    {
        admin, 
        approver
    }
}
