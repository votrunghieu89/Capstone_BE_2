namespace Capstone_2_BE.DTOs.Authentication
{
    public class LoginResultDTO
    {
        public Guid Id { get; set; }
        public string accessToken { get; set; }
        public string refressToken { get; set; }
        
    }
}
