using System;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs;
using Capstone_2_BE.DTOs.Authentication;
using Capstone_2_BE.Enums;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Services;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class AuthenticatioNTest
    {
        private static AuthenticationService CreateSut(IAuthenticationRepo repo)
            => new AuthenticationService(token: null!, authenticationDAL: repo, redis: null!, email: null!, google: null!);

        [Fact]
        public void GenerateOTP_DefaultLength_IsSixDigits()
        {
            var otp = AuthenticationService.GenerateOTP();
            Assert.Equal(6, otp.Length);
            Assert.All(otp.ToCharArray(), c => Assert.InRange(c, '0', '9'));
        }

        [Fact]
        public void GenerateOTP_LengthMustBePositive_Throws()
        {
            Assert.Throws<ArgumentException>(() => AuthenticationService.GenerateOTP(0));
        }

        [Fact]
        public async Task IsEmailExist_WhenRepoReturnsId_Returns200AndId()
        {
            var repo = new Mock<IAuthenticationRepo>();
            var id = Guid.NewGuid();
            repo.Setup(r => r.isEmailExist("a@b.com")).ReturnsAsync(id);

            var sut = CreateSut(repo.Object);
            var result = await sut.IsEmailExist("a@b.com");

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(id, result.Data);
        }

        [Fact]
        public async Task IsEmailExist_WhenRepoReturnsNull_Returns404()
        {
            var repo = new Mock<IAuthenticationRepo>();
            repo.Setup(r => r.isEmailExist("missing@b.com")).ReturnsAsync((Guid?)null);

            var sut = CreateSut(repo.Object);
            var result = await sut.IsEmailExist("missing@b.com");

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Email không tồn tại", result.Error);
        }

        [Fact]
        public async Task ChangePassword_WhenConfirmMismatch_Returns400_AndDoesNotCallRepo()
        {
            var repo = new Mock<IAuthenticationRepo>(MockBehavior.Strict);
            var sut = CreateSut(repo.Object);

            var dto = new ChangePasswordDTO
            {
                Id = Guid.NewGuid(),
                OldPassword = "old",
                NewPassword = "new1",
                ConfirmPassword = "new2"
            };

            var result = await sut.ChangePassword(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Mật khẩu xác nhận không khớp", result.Error);
            repo.VerifyAll();
        }

        [Fact]
        public async Task ChangePassword_WhenRepoReturnsTrue_Returns200()
        {
            var repo = new Mock<IAuthenticationRepo>();
            var sut = CreateSut(repo.Object);

            var dto = new ChangePasswordDTO
            {
                Id = Guid.NewGuid(),
                OldPassword = "old",
                NewPassword = "new",
                ConfirmPassword = "new"
            };

            repo.Setup(r => r.ChangePassword(dto)).ReturnsAsync(true);

            var result = await sut.ChangePassword(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Đổi mật khẩu thành công", result.Data);
            repo.Verify(r => r.ChangePassword(dto), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_WhenRepoReturnsFalse_Returns400()
        {
            var repo = new Mock<IAuthenticationRepo>();
            var sut = CreateSut(repo.Object);

            var dto = new ChangePasswordDTO
            {
                Id = Guid.NewGuid(),
                OldPassword = "old",
                NewPassword = "new",
                ConfirmPassword = "new"
            };

            repo.Setup(r => r.ChangePassword(dto)).ReturnsAsync(false);

            var result = await sut.ChangePassword(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Mật khẩu cũ không đúng hoặc tài khoản không tồn tại", result.Error);
        }

        [Theory]
        [InlineData(AuthenticationEnum.Register.Success, true, 200, "Đăng kí thành công")]
        [InlineData(AuthenticationEnum.Register.Fail, false, 400, "Đăng kí thất bại")]
        public async Task RegisterTechnician_MapsRepoResult(AuthenticationEnum.Register repoResult, bool expectSuccess, int expectStatus, string expectMessage)
        {
            var repo = new Mock<IAuthenticationRepo>();
            var sut = CreateSut(repo.Object);

            var input = new RegisterFixerDTO
            {
                Email = "t@ex.com",
                Password = "pw",
                FullName = "Tech",
                PhoneNumber = "0123",
                Address = "addr",
                CityId = Guid.NewGuid(),
                Latitude = null,
                Longitude = null
            };

            repo.Setup(r => r.RegisterTechnician(input)).ReturnsAsync(repoResult);

            var result = await sut.RegisterTechnician(input);

            Assert.Equal(expectSuccess, result.IsSuccess);
            Assert.Equal(expectStatus, result.StatusCode);
            Assert.Equal(expectMessage, expectSuccess ? result.Data : result.Error);
        }

        [Fact]
        public async Task RegisterAccountAdmin_WhenRepoReturnsTrue_Returns201()
        {
            var repo = new Mock<IAuthenticationRepo>();
            var sut = CreateSut(repo.Object);

            var dto = new LoginDTO { Email = "admin@b.com", Password = "pw" };
            repo.Setup(r => r.RegisterAccountAdmin(dto.Email, dto.Password)).ReturnsAsync(true);

            var result = await sut.RegisterAccountAdmin(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Tạo tài khoản admin thành công", result.Data);
        }

        [Fact]
        public async Task UpdateOnlineStatus_WhenInvalidValue_Returns400_AndDoesNotCallRepo()
        {
            var repo = new Mock<IAuthenticationRepo>(MockBehavior.Strict);
            var sut = CreateSut(repo.Object);

            var dto = new UpdateOnlineStatusDTO
            {
                AccountId = Guid.NewGuid(),
                IsOnline = 2
            };

            var result = await sut.UpdateOnlineStatus(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            repo.VerifyAll();
        }

        [Fact]
        public async Task UpdateOnlineStatus_WhenRepoReturnsTrue_AndIsOnline1_Returns200OnlineMessage()
        {
            var repo = new Mock<IAuthenticationRepo>();
            var sut = CreateSut(repo.Object);

            var dto = new UpdateOnlineStatusDTO { AccountId = Guid.NewGuid(), IsOnline = 1 };
            repo.Setup(r => r.UpdateOnlineStatus(dto.AccountId, dto.IsOnline)).ReturnsAsync(true);

            var result = await sut.UpdateOnlineStatus(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Đã chuyển sang trạng thái Online", result.Data);
        }

        [Fact]
        public async Task UpdateOnlineStatus_WhenRepoReturnsFalse_Returns404()
        {
            var repo = new Mock<IAuthenticationRepo>();
            var sut = CreateSut(repo.Object);

            var dto = new UpdateOnlineStatusDTO { AccountId = Guid.NewGuid(), IsOnline = 0 };
            repo.Setup(r => r.UpdateOnlineStatus(dto.AccountId, dto.IsOnline)).ReturnsAsync(false);

            var result = await sut.UpdateOnlineStatus(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không tìm thấy tài khoản hoặc cập nhật thất bại", result.Error);
        }

        [Fact]
        public async Task Login_WhenRepoReturnsWrong_Returns401_AndDoesNotUseTokenOrRedis()
        {
            var repo = new Mock<IAuthenticationRepo>();
            var sut = CreateSut(repo.Object);

            var login = new LoginDTO { Email = "a@b.com", Password = "bad" };

            repo.Setup(r => r.Login(login.Email, login.Password)).ReturnsAsync(new LoginResponseDTO
            {
                LoginStatus = AuthenticationEnum.Login.Wrong
            });

            var result = await sut.Login(login);

            Assert.False(result.IsSuccess);
            Assert.Equal(401, result.StatusCode);
            Assert.Equal("Sai tên đăng nhập hoặc mật khẩu", result.Error);
        }

        [Fact]
        public async Task Login_WhenRepoReturnsBanned_Returns404()
        {
            var repo = new Mock<IAuthenticationRepo>();
            var sut = CreateSut(repo.Object);

            var login = new LoginDTO { Email = "a@b.com", Password = "pw" };

            repo.Setup(r => r.Login(login.Email, login.Password)).ReturnsAsync(new LoginResponseDTO
            {
                LoginStatus = AuthenticationEnum.Login.Banned
            });

            var result = await sut.Login(login);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Tài khoản đã bị khoá", result.Error);
        }

        [Fact]
        public async Task Login_WhenRepoReturnsFail_Returns500()
        {
            var repo = new Mock<IAuthenticationRepo>();
            var sut = CreateSut(repo.Object);

            var login = new LoginDTO { Email = "a@b.com", Password = "pw" };

            repo.Setup(r => r.Login(login.Email, login.Password)).ReturnsAsync(new LoginResponseDTO
            {
                LoginStatus = AuthenticationEnum.Login.Fail
            });

            var result = await sut.Login(login);

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Đăng nhập thất bại", result.Error);
        }
    }
}
