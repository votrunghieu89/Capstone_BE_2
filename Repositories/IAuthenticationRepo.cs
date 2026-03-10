using Capstone_2_BE.DTOs.Authentication;

namespace Capstone_2_BE.Repositories
{
    public interface IAuthenticationRepo
    {
        Task<LoginResponseDTO> Login(string email, string password);
        Task<int> isEmailExist(string email);
        Task<bool> RegisterStudent(RegisterCustomerDTO authRegisterDTO);
        Task<bool> RegisterTeacher(RegisterFixerDTO authRegisterDTO);
        Task<bool> RegisterAccountAdmin(string email, string password);
        Task<bool> ChangePassword(ChangePasswordDTO changePasswordDTO);
        Task<string> getNewAccessToken(string RefressToken);
        Task<bool> Logout(int accountId);
        Task<bool> verifyOTP(string Email, string otp);
    }
}