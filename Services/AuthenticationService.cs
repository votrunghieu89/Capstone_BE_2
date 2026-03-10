using Capstone_2_BE.DALs;
using Capstone_2_BE.DTOs;
using Capstone_2_BE.DTOs.Authentication;
using Capstone_2_BE.Enums;
using Capstone_2_BE.Securities;

namespace Capstone_2_BE.Services
{
    public class AuthenticationService
    {
        private readonly Token _token;
        private readonly AuthenticationDAL _authenticationDAL;

        public AuthenticationService(Token token, AuthenticationDAL authenticationDAL)
        {
            _token = token;
            _authenticationDAL = authenticationDAL;
        }
        public async Task<Result<LoginResultDTO>> Login(LoginDTO loginDTO)
        {
            var loginResult = await _authenticationDAL.Login(loginDTO.Email, loginDTO.Password);
            switch (loginResult.LoginStatus)
            {
                case AuthenticationEnum.Login.Success:
                    var accessToken = _token.generateAccessToken(loginResult.Id, loginResult.Role, loginResult.Email);
                    var refressToken = _token.generateRefreshToken();
                    return Result<LoginResultDTO>.Success(new LoginResultDTO
                    {
                        Id = loginResult.Id,
                        accessToken = accessToken,
                        refressToken = refressToken
                    });
                case AuthenticationEnum.Login.Wrong:
                    return Result<LoginResultDTO>.Failure("Sai tên đăng nhập hoặc mật khẩu", 401);
                case AuthenticationEnum.Login.Banned:
                    return Result<LoginResultDTO>.Failure("Tài khoản đã bị khoá", 404);
                case AuthenticationEnum.Login.Fail:
                default:
                    return Result<LoginResultDTO>.Failure("Đăng nhập thất bại", 500);

            }
        }
    }
}
