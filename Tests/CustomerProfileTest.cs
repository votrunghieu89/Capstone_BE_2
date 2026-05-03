using System;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Customer.Profile;
using Capstone_2_BE.Repositories.Customer;
using Capstone_2_BE.Services.Customer;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class CustomerProfileTest
    {
        private static CustomerProfileService CreateSut(Mock<ICustomerProfileRepo> repo, Mock<ILogger<CustomerProfileService>> logger)
            => new CustomerProfileService(repo.Object, aws: null!, logger.Object);

        [Fact]
        public async Task GetCustomerProfile_WhenRepoReturnsNull_Returns404()
        {
            var repo = new Mock<ICustomerProfileRepo>();
            var logger = new Mock<ILogger<CustomerProfileService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.GetCustomerProfile(id)).ReturnsAsync((CustomerProfileViewDTO?)null);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCustomerProfile(id);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetCustomerProfile_WhenAvatarUrlEmpty_Returns200_AndDoesNotNeedAws()
        {
            var repo = new Mock<ICustomerProfileRepo>();
            var logger = new Mock<ILogger<CustomerProfileService>>();

            var id = Guid.NewGuid();
            var profile = new CustomerProfileViewDTO { Email = "a@b.com", FullName = "A", PhoneNumber = "1", AvatarURL = string.Empty, CreateAt = DateTime.UtcNow };
            repo.Setup(r => r.GetCustomerProfile(id)).ReturnsAsync(profile);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCustomerProfile(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(profile, result.Data);
        }

        [Fact]
        public async Task GetCustomerProfile_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ICustomerProfileRepo>();
            var logger = new Mock<ILogger<CustomerProfileService>>();

            repo.Setup(r => r.GetCustomerProfile(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCustomerProfile(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task UpdateCustomerProfile_WhenNoAvatar_RepoReturnsTrue_Returns200()
        {
            var repo = new Mock<ICustomerProfileRepo>();
            var logger = new Mock<ILogger<CustomerProfileService>>();

            repo.Setup(r => r.UpdateTechnicianProfile(It.IsAny<CustomerProfileUpdateDALDTO>())).ReturnsAsync(true);

            var sut = CreateSut(repo, logger);
            var dto = new CustomerProfileUpdateDTO { Id = Guid.NewGuid(), FullName = "A", PhoneNumber = "1", AvatarURl = null };

            var result = await sut.UpdateCustomerProfile(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            repo.Verify(r => r.UpdateTechnicianProfile(It.Is<CustomerProfileUpdateDALDTO>(d => d.Id == dto.Id && d.FullName == dto.FullName && d.PhoneNumber == dto.PhoneNumber)), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomerProfile_WhenNoAvatar_RepoReturnsFalse_Returns400()
        {
            var repo = new Mock<ICustomerProfileRepo>();
            var logger = new Mock<ILogger<CustomerProfileService>>();

            repo.Setup(r => r.UpdateTechnicianProfile(It.IsAny<CustomerProfileUpdateDALDTO>())).ReturnsAsync(false);

            var sut = CreateSut(repo, logger);
            var dto = new CustomerProfileUpdateDTO { Id = Guid.NewGuid(), FullName = "A", PhoneNumber = "1", AvatarURl = null };

            var result = await sut.UpdateCustomerProfile(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UpdateCustomerProfile_WhenNoAvatar_RepoThrows_Returns500()
        {
            var repo = new Mock<ICustomerProfileRepo>();
            var logger = new Mock<ILogger<CustomerProfileService>>();

            repo.Setup(r => r.UpdateTechnicianProfile(It.IsAny<CustomerProfileUpdateDALDTO>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var dto = new CustomerProfileUpdateDTO { Id = Guid.NewGuid(), FullName = "A", PhoneNumber = "1", AvatarURl = null };

            var result = await sut.UpdateCustomerProfile(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
    }
}
