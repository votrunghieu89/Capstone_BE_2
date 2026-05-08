using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Capstone_2_BE.Models
{
    [Table("InvoiceItems")]
    public class InvoiceItemsModel
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; set; }

        [ForeignKey("Invoices")]
        [Column("InvoiceId")]
        public Guid InvoiceId { get; set; }

        [Column("MaterialName")]
        public string MaterialName { get; set; } = string.Empty;

        [Column("Price", TypeName = "decimal(12,2)")]
        public decimal Price { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }

        [Column("Subtotal", TypeName = "decimal(12,2)")]
        public decimal Subtotal { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        public InvoicesModel Invoices { get; set; }
    }
}
