namespace Capstone_2_BE.DTOs.Customer.Order
{
    public class OrderUpdateFormDTO
    {
        public Guid OrderId { get; set; }
        public Guid TechnicianId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? videoUrl { get; set; } = null;
        public List<IFormFile> ImageUrls { get; set; } = new List<IFormFile>();
    }
}
