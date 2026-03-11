namespace Capstone_2_BE.DTOs.Technician.Profile
{
    public class TechnicianProfileUpdateDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public IFormFile? AvatarURl { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public Guid ServiceId { get; set; }
    }
}
