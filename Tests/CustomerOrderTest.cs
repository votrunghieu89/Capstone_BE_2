using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Customer.Order;
using Capstone_2_BE.DTOs.Notification;
using Capstone_2_BE.DTOs.Technician.Orders;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Repositories.Customer;
using Capstone_2_BE.Repositories.Technician;
using Capstone_2_BE.Services.Customer;
using Capstone_2_BE.Settings;
using Capstone_2_BE.Socket;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class CustomerOrderTest
    {
        private static CustomerOrderService CreateSut(
            Mock<ITechnicianProfileRepo> technicianRepo,
            Mock<ICustomerOrderRepo> repo,
            Mock<ILogger<CustomerOrderService>> logger,
            Mock<IHubContext<NotificationHub>> hub,
            Mock<INotificationRepo> notificationRepo)
            => new CustomerOrderService(
                technicianRepo.Object,
                repo.Object,
                logger.Object,
                hub.Object,
                notificationRepo.Object,
                aws: null!,
                aIEstimationTime: null!);

        [Fact]
        public async Task GetCurrentOrders_WhenRepoReturnsNull_ReturnsEmptyList200()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.GetCurrentOrders(id)).ReturnsAsync((List<OrderOverviewDTO>?)null);

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.GetCurrentOrders(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetCurrentOrders_WhenRepoThrows_Returns500()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.GetCurrentOrders(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.GetCurrentOrders(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task GetInProgressOrders_WhenRepoReturnsEmpty_ReturnsEmptyList200()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.GetInProgressOrders(It.IsAny<Guid>())).ReturnsAsync(new List<OrderOverviewDTO>());

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.GetInProgressOrders(Guid.NewGuid());

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetOrderHistory_WhenRepoReturnsEmpty_ReturnsEmptyList200()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.GetOrderHistory(id)).ReturnsAsync(new List<OrderOverviewDTO>());

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.GetOrderHistory(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetCancalledOrder_WhenRepoReturnsNull_ReturnsEmptyList200()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.GetCancalledOrder(It.IsAny<Guid>())).ReturnsAsync((List<OrderOverviewDTO>?)null);

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.GetCancalledOrder(Guid.NewGuid());

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetRejectedOrder_WhenRepoReturnsEmpty_ReturnsEmptyList200()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.GetRejectedOrder(It.IsAny<Guid>())).ReturnsAsync(new List<OrderOverviewDTO>());

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.GetRejectedOrder(Guid.NewGuid());

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task CancelOrder_WhenRepoReturnsNull_Returns400()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.CancelOrder(It.IsAny<OrderActionDTO>())).ReturnsAsync((OrderActionResDTO?)null);

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.CancelOrder(new OrderActionDTO { OrderId = Guid.NewGuid(), technicianId = Guid.NewGuid() });

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CancelOrder_WhenNotificationInsertFails_Returns400_AndDoesNotSendHub()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();

            var clients = new Mock<IHubClients>();
            var clientProxy = new Mock<IClientProxy>();
            clients.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxy.Object);

            var hub = new Mock<IHubContext<NotificationHub>>();
            hub.SetupGet(h => h.Clients).Returns(clients.Object);

            var notificationRepo = new Mock<INotificationRepo>();

            var actionRes = new OrderActionResDTO
            {
                OrderId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OrderName = "o",
                CreatedAt = DateTime.UtcNow
            };

            repo.Setup(r => r.CancelOrder(It.IsAny<OrderActionDTO>())).ReturnsAsync(actionRes);
            notificationRepo.Setup(n => n.InsertNewNotification(It.IsAny<InsertNewNotificationDTO>())).ReturnsAsync(false);

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.CancelOrder(new OrderActionDTO { OrderId = actionRes.OrderId, technicianId = actionRes.ReceiverId });

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            clientProxy.Verify(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default), Times.Never);
        }

        [Fact]
        public async Task CancelOrder_WhenRepoThrows_Returns500()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.CancelOrder(It.IsAny<OrderActionDTO>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.CancelOrder(new OrderActionDTO { OrderId = Guid.NewGuid(), technicianId = Guid.NewGuid() });

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task ConfirmCompletedOrder_WhenRepoReturnsNull_Returns400()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.ConfirmCompletedOrder(It.IsAny<OrderActionDTO>())).ReturnsAsync((OrderActionResDTO?)null);

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.ConfirmCompletedOrder(new OrderActionDTO { OrderId = Guid.NewGuid(), technicianId = Guid.NewGuid() });

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task ConfirmCompletedOrder_WhenRepoThrows_Returns500()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>();
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.ConfirmCompletedOrder(It.IsAny<OrderActionDTO>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);
            var result = await sut.ConfirmCompletedOrder(new OrderActionDTO { OrderId = Guid.NewGuid(), technicianId = Guid.NewGuid() });

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task UpdateOrder_WhenTitleNull_Returns500_AndDoesNotCallRepo()
        {
            var technicianRepo = new Mock<ITechnicianProfileRepo>();
            var repo = new Mock<ICustomerOrderRepo>(MockBehavior.Strict);
            var logger = new Mock<ILogger<CustomerOrderService>>();
            var hub = new Mock<IHubContext<NotificationHub>>();
            var notificationRepo = new Mock<INotificationRepo>();

            var sut = CreateSut(technicianRepo, repo, logger, hub, notificationRepo);

            var dto = new OrderUpdateFormDTO
            {
                OrderId = Guid.NewGuid(),
                Title = null,
                Description = "d",
                videoUrl = null,
                ImageUrls = new List<Microsoft.AspNetCore.Http.IFormFile>()
            };

            var result = await sut.UpdateOrder(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            repo.VerifyAll();
        }
    }
}
