namespace Capstone_2_BE.DTOs.Customer.AutoFind
{
    public class AutoFindFixerResDTO
    {
        public Guid TechnicianId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string avatarURL { get; set; } = string.Empty;   
        public string ServiceName { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal Score { get; set; }
        public decimal Total { get; set; }
    }
}
