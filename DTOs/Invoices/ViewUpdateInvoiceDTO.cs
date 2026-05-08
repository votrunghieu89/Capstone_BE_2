namespace Capstone_2_BE.DTOs.Invoices
{
    public class ViewUpdateInvoiceDTO
    {
        public Guid OrderId { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal LaborCost { get; set; }
        public decimal TotalAmount { get; set; }
        public string BankCode { get; set; }
        public string BankAccount { get; set; }
        public string BankAccountName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ViewMaterialItemDTO> Materials { get; set; } = new List<ViewMaterialItemDTO>();
        public class ViewMaterialItemDTO
        {
            public string MaterialName { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal { get; set; }
        }
    }
}
