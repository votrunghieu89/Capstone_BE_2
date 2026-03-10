using Capstone_2_BE.DTOs.Authentication;
using Capstone_2_BE.Enums;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Securities;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Capstone_2_BE.DALs
{
    public class AuthenticationDAL : IAuthenticationRepo
    {
        public readonly AppDbContext _context;
        public readonly ILogger<AuthenticationDAL> _logger;
        public readonly Token _token;

        public AuthenticationDAL(AppDbContext context, ILogger<AuthenticationDAL> logger, Token token)
        {
            _context = context;
            _logger = logger;
            _token = token;
        }

        public Task<bool> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            throw new NotImplementedException();
        }

        public Task<string> getNewAccessToken(string RefressToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> isEmailExist(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<LoginResponseDTO> Login(string email, string password)
        {
            try
            {
                var isExsist =  await _context.AccountsModel.Where(a => a.Email ==  email).FirstOrDefaultAsync();
                if (isExsist != null) {
                    return new LoginResponseDTO()
                    {
                        LoginStatus = AuthenticationEnum.Login.Wrong,
                    };
                }
                if(isExsist.IsActive == 0)
                {
                    return new LoginResponseDTO()
                    {
                        LoginStatus = AuthenticationEnum.Login.Banned,
                    };
                }
                bool checkPassword = Hash.VerifyPassword(password, isExsist.Password);
                if (!checkPassword)
                {
                    return new LoginResponseDTO()
                    {
                        LoginStatus = AuthenticationEnum.Login.Wrong,
                    };
                }
                //bool setRefresh = await _redis.SetStringAsync($"RefressToken_{user.AccountId}", refreshToken, TimeSpan.FromDays(7));
                return new LoginResponseDTO()
                {
                    Id = isExsist.Id,
                    Role = isExsist.Role,
                    Email = email,
                    LoginStatus = AuthenticationEnum.Login.Success
                };
            }
            catch (Exception ex) {
                return new LoginResponseDTO()
                {
                    LoginStatus = AuthenticationEnum.Login.Fail,
                };
            }
        }

        public Task<bool> Logout(int accountId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegisterAccountAdmin(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegisterStudent(RegisterCustomerDTO authRegisterDTO)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegisterTeacher(RegisterFixerDTO authRegisterDTO)
        {
            throw new NotImplementedException();
        }

        public Task<bool> verifyOTP(string Email, string otp)
        {
            throw new NotImplementedException();
        }
    }
}
