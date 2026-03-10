using Capstone_2_BE.DTOs.Authentication;
using Capstone_2_BE.Enums;

namespace Capstone_2_BE.Repositories
{
    public interface IAuthenticationRepo
    {
        Task<LoginResponseDTO> Login(string email, string password);
        Task<int> isEmailExist(string email);
        Task<AuthenticationEnum.Register> RegisterCustomer(RegisterCustomerDTO authRegisterDTO);
        Task<AuthenticationEnum.Register> RegisterTechnician(RegisterFixerDTO authRegisterDTO);
        Task<bool> RegisterAccountAdmin(string email, string password);
        Task<bool> ChangePassword(ChangePasswordDTO changePasswordDTO);
        Task<bool> ForgetPassword(int accountId, string password);
        Task<string> getNewAccessToken(string RefressToken);
        Task<bool> Logout(int accountId);
        Task<bool> verifyOTP(string Email, string otp);
    }
}