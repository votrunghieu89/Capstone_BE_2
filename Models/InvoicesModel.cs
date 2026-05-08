using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_2_BE.Models
{
    [Table("Invoices")]
    public class InvoicesModel
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; set; }

        [ForeignKey("Order")]
        [Column("OrderId")]
        public Guid OrderId { get; set; }

        [Column("LaborCost", TypeName = "decimal(12,2)")]
        public decimal LaborCost { get; set; }

        [Column("TotalAmount", TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("BankCode")]
        public string? BankCode { get; set; }

        [Column("BankAccount")]
        public string? BankAccount { get; set; }

        [Column("BankAccountName")]
        public string? BankAccountName { get; set; }

        [Column("PaymentStatus")]
        public int PaymentStatus { get; set; } // 0: Chưa thanh toán, 1: Đã thanh toán

        public OrderrModel Order { get; set; }
        public ICollection<InvoiceItemsModel> InvoiceItems { get; set; }
    }
}
