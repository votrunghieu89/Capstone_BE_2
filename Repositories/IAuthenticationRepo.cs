using Capstone_2_BE.DTOs.Authentication;
using System.Runtime.InteropServices;

namespace Capstone_2_BE.Repositories
{
    public interface IAuthenticationRepo
    {
        Task<LoginResponseDTO> Login(string email, string password);
        Task<int> isEmailExist(string email);
        Task<bool> RegisterStudent(RegisterCustomerDTO authRegisterDTO, string IpAddress);
        Task<bool> RegisterTeacher(RegisterFixerDTO authRegisterDTO, string IpAddress);
    }
}
