namespace Capstone_2_BE.DTOs.Invoices
{
    public class ViewAllInvoice
    {
        public Guid InvoiceId { get; set; }
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
        public string TechnicianName { get; set; }
        public int PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
