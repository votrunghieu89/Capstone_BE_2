using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Technician.Statistic;
using Capstone_2_BE.Repositories.Technician;
using Capstone_2_BE.Services.Technician;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class TechnicianStatisticTest
    {
        private static TechnicianStatisticService CreateSut(Mock<ITechnicianStatisticRepo> repo, Mock<ILogger<TechnicianStatisticService>> logger)
            => new TechnicianStatisticService(repo.Object, logger.Object);

        [Fact]
        public async Task GetCompletedOrdersByWeek_WhenRepoReturnsData_Returns200AndSameData()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            var from = DateTime.UtcNow.Date.AddDays(-6);
            var to = DateTime.UtcNow.Date;
            var data = new List<StatisticItemDTO> { new StatisticItemDTO { Label = "d", Value = 1 } };

            repo.Setup(r => r.GetCompletedOrdersByWeek(techId, from, to)).ReturnsAsync(data);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCompletedOrdersByWeek(techId, from, to);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(data, result.Data);
        }

        [Fact]
        public async Task GetCompletedOrdersByWeek_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            repo.Setup(r => r.GetCompletedOrdersByWeek(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCompletedOrdersByWeek(Guid.NewGuid(), DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Lỗi lấy dữ liệu thống kê", result.Error);
        }

        [Fact]
        public async Task GetRatingOverview_WhenRepoReturnsDto_Returns200()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            var dto = new RatingOverviewDTO { TotalRating = 10, AvgScore = 4.5m };
            repo.Setup(r => r.GetRatingOverview(techId)).ReturnsAsync(dto);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetRatingOverview(techId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(dto, result.Data);
        }

        [Fact]
        public async Task GetCanceledOrdersTotal_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            repo.Setup(r => r.GetCanceledOrdersTotal(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCanceledOrdersTotal(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Lỗi lấy dữ liệu thống kê", result.Error);
        }

        [Fact]
        public async Task GetCompletedOrdersByDays_WhenRepoReturnsCount_Returns200AndCount()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            var date = new DateTime(2026, 1, 1);
            repo.Setup(r => r.GetCompletedOrdersByDays(techId, date)).ReturnsAsync(3);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCompletedOrdersByDays(techId, date);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(3, result.Data);
        }

        [Fact]
        public async Task GetAvgRate_WhenRepoReturnsValue_Returns200AndValue()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            repo.Setup(r => r.getAvgRate(techId)).ReturnsAsync(4.2m);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetAvgRate(techId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(4.2m, result.Data);
        }
    }
}
