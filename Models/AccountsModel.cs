using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_2_BE.Models
{
    [Table("Account")]
    public class AccountsModel
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("Email")]
        public string Name { get; set; }
        [Column("Password")]
        public string Password { get; set; }
        [Column("Role")]
        public string Role { get; set; }
        [Column("IsActive")]
        public int IsActive { get; set; }
        [Column("CreateAt")]
        public DateTime CreateAt { get; set; }

        [Column("UpdateAt")]
        public DateTime UpdateAt { get; set; }

        public TechnicianProfileModel TechnicianProfile { get; set; }
        public CustomerProfileModel CustomerProfile { get; set; }

        public ICollection<OrderStatusHistoryModel> OrderStatusHistory { get; set; }


    }
}
