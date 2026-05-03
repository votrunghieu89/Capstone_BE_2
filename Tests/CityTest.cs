using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.City;
using Capstone_2_BE.Repositories.Administrator;
using Capstone_2_BE.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class CityTest
    {
        private static CityService CreateSut(Mock<ICityRepo> repo, Mock<ILogger<CityService>> logger)
            => new CityService(repo.Object, logger.Object);

        [Fact]
        public async Task CreateCity_WhenRepoReturnsTrue_Returns201()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            repo.Setup(r => r.CreateCity("Da Nang")).ReturnsAsync(true);

            var sut = CreateSut(repo, logger);
            var result = await sut.CreateCity(new CreateCityDTO { CityName = "Da Nang" });

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Tạo thành phố thành công", result.Data);
        }

        [Fact]
        public async Task CreateCity_WhenRepoReturnsFalse_Returns400()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            repo.Setup(r => r.CreateCity("Da Nang")).ReturnsAsync(false);

            var sut = CreateSut(repo, logger);
            var result = await sut.CreateCity(new CreateCityDTO { CityName = "Da Nang" });

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Tạo thành phố thất bại", result.Error);
        }

        [Fact]
        public async Task UpdateCity_WhenRepoReturnsTrue_Returns200()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.UpdateCity(id, "HCM")).ReturnsAsync(true);

            var sut = CreateSut(repo, logger);
            var result = await sut.UpdateCity(id, new CreateCityDTO { CityName = "HCM" });

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Cập nhật thành phố thành công", result.Data);
        }

        [Fact]
        public async Task UpdateCity_WhenRepoReturnsFalse_Returns400()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.UpdateCity(id, "HCM")).ReturnsAsync(false);

            var sut = CreateSut(repo, logger);
            var result = await sut.UpdateCity(id, new CreateCityDTO { CityName = "HCM" });

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Cập nhật thành phố thất bại", result.Error);
        }

        [Fact]
        public async Task DeleteCity_WhenRepoReturnsTrue_Returns200()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.DeleteCity(id)).ReturnsAsync(true);

            var sut = CreateSut(repo, logger);
            var result = await sut.DeleteCity(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Xóa thành phố thành công", result.Data);
        }

        [Fact]
        public async Task DeleteCity_WhenRepoReturnsFalse_Returns400()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.DeleteCity(id)).ReturnsAsync(false);

            var sut = CreateSut(repo, logger);
            var result = await sut.DeleteCity(id);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Xóa thành phố thất bại", result.Error);
        }

        [Fact]
        public async Task ViewAllCities_WhenRepoReturnsList_Returns200AndSameList()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            var list = new List<ViewAllCities> { new ViewAllCities { CityId = Guid.NewGuid(), CityName = "HN" } };
            repo.Setup(r => r.ViewAllCities()).ReturnsAsync(list);

            var sut = CreateSut(repo, logger);
            var result = await sut.ViewAllCities();

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(list, result.Data);
        }

        [Fact]
        public async Task GetCityName_WhenRepoReturnsEmpty_Returns404()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.GetCityName(id)).ReturnsAsync(string.Empty);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCityName(id);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không tìm thấy thành phố", result.Error);
        }

        [Fact]
        public async Task GetCityName_WhenRepoReturnsName_Returns200AndName()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.GetCityName(id)).ReturnsAsync("Hà Nội");

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCityName(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Hà Nội", result.Data);
        }

        [Fact]
        public async Task GetCityID_WhenCityNameIsWhitespace_Returns400()
        {
            var repo = new Mock<ICityRepo>(MockBehavior.Strict);
            var logger = new Mock<ILogger<CityService>>();

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCityID("  ");

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Tên thành phố không hợp lệ", result.Error);
        }

        [Fact]
        public async Task GetCityID_WhenRepoReturnsEmptyGuid_Returns404()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            repo.Setup(r => r.GetCityID("HCM")).ReturnsAsync(Guid.Empty);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCityID("HCM");

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Không tìm thấy thành phố", result.Error);
        }

        [Fact]
        public async Task GetCityID_WhenRepoReturnsGuid_Returns200()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.GetCityID("HCM")).ReturnsAsync(id);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCityID("HCM");

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(id, result.Data);
        }

        [Fact]
        public async Task ViewAllCities_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ICityRepo>();
            var logger = new Mock<ILogger<CityService>>();

            repo.Setup(r => r.ViewAllCities()).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.ViewAllCities();

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
    }
}
