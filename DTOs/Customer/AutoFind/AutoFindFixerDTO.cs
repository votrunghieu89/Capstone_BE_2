namespace Capstone_2_BE.DTOs.Customer.AutoFind
{
    public class AutoFindFixerDTO
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string City { get; set; }
        public Guid ServiceId { get; set; }
    }
}
