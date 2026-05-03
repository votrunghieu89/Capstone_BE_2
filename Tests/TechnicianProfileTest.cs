using System;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Technician.Profile;
using Capstone_2_BE.Repositories.Technician;
using Capstone_2_BE.Services.Technician;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class TechnicianProfileTest
    {
        [Fact]
        public async Task GetTechnicianProfile_WhenRepoReturnsNull_Returns404()
        {
            var repo = new Mock<ITechnicianProfileRepo>();
            var logger = new Mock<ILogger<TechnicianProfileService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.GetTechnicianProfile(id)).ReturnsAsync((TechnicianProfileViewDTO?)null);

            var sut = new TechnicianProfileService(repo.Object, aws: null!, logger.Object);
            var result = await sut.GetTechnicianProfile(id);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task UpdateTechnicianProfile_WhenRepoReturnsFalse_Returns400()
        {
            var repo = new Mock<ITechnicianProfileRepo>();
            var logger = new Mock<ILogger<TechnicianProfileService>>();

            var dto = new TechnicianProfileUpdateDTO
            {
                Id = Guid.NewGuid(),
                FullName = "T",
                PhoneNumber = "1",
                Address = "addr",
                CityId = Guid.NewGuid(),
                Latitude = "10.1",
                Longitude = "106.2",
                ServiceId = Guid.NewGuid(),
                Description = "d",
                Experiences = 1,
                AvatarURl = null
            };

            repo.Setup(r => r.UpdateTechnicianProfile(It.IsAny<TechnicianProfileUpdateDALDTO>())).ReturnsAsync(false);

            var sut = new TechnicianProfileService(repo.Object, aws: null!, logger.Object);
            var result = await sut.UpdateTechnicianProfile(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }
    }
}
