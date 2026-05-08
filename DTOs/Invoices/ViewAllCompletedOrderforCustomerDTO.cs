namespace Capstone_2_BE.DTOs.Invoices
{
    public class ViewAllCompletedOrderforCustomerDTO
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid TechnicianID { get; set; }
        public string TechnicianName { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
