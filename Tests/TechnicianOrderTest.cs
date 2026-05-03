using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Technician.Orders;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Repositories.Technician;
using Capstone_2_BE.Services.Technician;
using Capstone_2_BE.Socket;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class TechnicianOrderTest
    {
        private static TechnicianOrderService CreateSut(
            Mock<ITechnicianOrderRepo> repo,
            Mock<ILogger<TechnicianOrderService>> logger,
            Mock<IHubContext<NotificationHub>> hub,
            Mock<INotificationRepo> notificationRepo)
            => new TechnicianOrderService(repo.Object, logger.Object, hub.Object, notificationRepo.Object, aws: null!);

        [Fact]
        public async Task StartOrder_WhenAlreadyHasInProgress_Returns400_AndDoesNotStart()
        {
            var repo = new Mock<ITechnicianOrderRepo>(MockBehavior.Strict);
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            var techId = Guid.NewGuid();
            repo.Setup(r => r.GetInProgressOrders(techId)).ReturnsAsync(new ViewOrderDTO());

            var sut = CreateSut(repo, logger, hub, notificationRepo);
            var dto = new OrderActionDTO { OrderId = Guid.NewGuid(), technicianId = techId };

            var result = await sut.StartOrder(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            repo.Verify(r => r.GetInProgressOrders(techId), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetConfirmingOrders_WhenRepoReturnsNull_ReturnsEmptyList200()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            var techId = Guid.NewGuid();
            repo.Setup(r => r.GetConfirmingOrders(techId)).ReturnsAsync((List<ViewOrderDTO>?)null);

            var sut = CreateSut(repo, logger, hub, notificationRepo);
            var result = await sut.GetConfirmingOrders(techId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }
    }
}
