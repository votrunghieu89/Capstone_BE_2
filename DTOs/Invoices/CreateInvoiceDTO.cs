namespace Capstone_2_BE.DTOs.Invoices
{
    public class CreateInvoiceDTO
    {
        public Guid OrderId { get; set; }
        public decimal LaborCost { get; set; }
        public string? BankCode { get; set; }
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }

        public List<MaterialItemDTO> Materials { get; set; } = new List<MaterialItemDTO>();
    }

    public class MaterialItemDTO
    {
        public string MaterialName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
