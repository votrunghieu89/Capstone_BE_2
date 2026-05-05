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
        public async Task GetCompletedOrdersByMonth_WhenRepoReturnsData_Returns200()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            var data = new List<StatisticItemDTO> { new StatisticItemDTO { Label = "m", Value = 2 } };
            repo.Setup(r => r.GetCompletedOrdersByMonth(techId, 2026)).ReturnsAsync(data);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCompletedOrdersByMonth(techId, 2026);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(data, result.Data);
        }

        [Fact]
        public async Task GetReceivedOrdersByWeek_WhenRepoReturnsData_Returns200()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            var from = DateTime.UtcNow.Date.AddDays(-6);
            var to = DateTime.UtcNow.Date;
            var data = new List<StatisticItemDTO> { new StatisticItemDTO { Label = "w", Value = 5 } };
            repo.Setup(r => r.GetReceivedOrdersByWeek(techId, from, to)).ReturnsAsync(data);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetReceivedOrdersByWeek(techId, from, to);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(data, result.Data);
        }

        [Fact]
        public async Task GetReceivedOrdersByMonth_WhenRepoReturnsData_Returns200()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            var data = new List<StatisticItemDTO> { new StatisticItemDTO { Label = "m", Value = 10 } };
            repo.Setup(r => r.GetReceivedOrdersByMonth(techId, 2026)).ReturnsAsync(data);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetReceivedOrdersByMonth(techId, 2026);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(data, result.Data);
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
        public async Task GetCanceledOrdersByWeek_WhenRepoReturnsData_Returns200()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            var from = DateTime.UtcNow.Date.AddDays(-6);
            var to = DateTime.UtcNow.Date;
            var data = new List<StatisticItemDTO> { new StatisticItemDTO { Label = "w", Value = 1 } };
            repo.Setup(r => r.GetCanceledOrdersByWeek(techId, from, to)).ReturnsAsync(data);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetCanceledOrdersByWeek(techId, from, to);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(data, result.Data);
        }

        [Fact]
        public async Task GetRejectedOrdersTotal_WhenRepoReturnsValue_Returns200()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            repo.Setup(r => r.GetRejectedOrdersTotal(techId)).ReturnsAsync(7);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetRejectedOrdersTotal(techId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(7, result.Data);
        }

        [Fact]
        public async Task GetRejectedOrdersByMonth_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            repo.Setup(r => r.GetRejectedOrdersByMonth(It.IsAny<Guid>(), It.IsAny<int>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.GetRejectedOrdersByMonth(Guid.NewGuid(), 2026);

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Lỗi lấy dữ liệu thống kê", result.Error);
        }

        [Fact]
        public async Task GetTodayReceivedOrders_WhenRepoReturnsValue_Returns200()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            repo.Setup(r => r.GetTodayReceivedOrders(techId)).ReturnsAsync(2);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetTodayReceivedOrders(techId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(2, result.Data);
        }

        [Fact]
        public async Task GetTodayCompletedOrders_WhenRepoReturnsValue_Returns200()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            repo.Setup(r => r.GetTodayCompletedOrders(techId)).ReturnsAsync(3);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetTodayCompletedOrders(techId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(3, result.Data);
        }

        [Fact]
        public async Task GetTotalCompletedOrders_WhenRepoReturnsValue_Returns200()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            repo.Setup(r => r.GetTotalCompletedOrders(techId)).ReturnsAsync(11);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetTotalCompletedOrders(techId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(11, result.Data);
        }

        [Fact]
        public async Task GetTotalOrders_WhenRepoReturnsValue_Returns200()
        {
            var repo = new Mock<ITechnicianStatisticRepo>();
            var logger = new Mock<ILogger<TechnicianStatisticService>>();

            var techId = Guid.NewGuid();
            repo.Setup(r => r.GetTotalOrders(techId)).ReturnsAsync(20);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetTotalOrders(techId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(20, result.Data);
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
