namespace Capstone_2_BE.DTOs.Invoices
{
    public class ViewDetailInvoiceDTO
    {
        public Guid InvoiceId { get; set; }
        public string NameCustomer { get; set; }
        public string NameTechnician { get; set; }
       
        public string ServiceName { get; set; }
        public string AdressOrder { get; set; }
        public string CityNameOrder { get; set; }
        public string CustomerPhone { get; set; }

        public List<ViewMaterialItemDTO> Materials { get; set; } = new List<ViewMaterialItemDTO>();

        public decimal LaborCost { get; set; }
        public decimal TotalAmount { get; set; }
        public string QRCode { get; set; }
        public int PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ViewMaterialItemDTO
    {
        public string MaterialName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}
