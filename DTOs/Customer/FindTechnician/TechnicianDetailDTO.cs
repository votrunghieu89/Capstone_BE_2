namespace Capstone_2_BE.DTOs.Customer.FindTechnician
{
    public class TechnicianDetailDTO
    {
         public Guid TechnicianId { get; set; }
        public Guid ServiceId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int OrderCount { get; set; }
        public int RatingCount { get; set; }
        public decimal AverageRating { get; set; }
        public string Status { get; set; }
        public double YearOfExperience { get; set; }
        public string? Address { get; set; }
        public string? CityName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Descritption { get; set; }
    }
}
