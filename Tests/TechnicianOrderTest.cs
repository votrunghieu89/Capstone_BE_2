using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs;
using Capstone_2_BE.DTOs.Notification;
using Capstone_2_BE.DTOs.Technician.Orders;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Repositories.Technician;
using Capstone_2_BE.Services.Technician;
using Capstone_2_BE.Settings;
using Capstone_2_BE.Socket;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
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
            Mock<INotificationRepo> notificationRepo,
            AWS aws)
            => new TechnicianOrderService(repo.Object, logger.Object, hub.Object, notificationRepo.Object, aws);

        private static Mock<IHubContext<NotificationHub>> CreateHub(out Mock<IClientProxy> clientProxy)
        {
            clientProxy = new Mock<IClientProxy>();
            var clients = new Mock<IHubClients>();
            clients.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxy.Object);

            var hub = new Mock<IHubContext<NotificationHub>>();
            hub.SetupGet(h => h.Clients).Returns(clients.Object);
            return hub;
        }

        private static AWS CreateAwsForReadOnly()
        {
            var settings = new Dictionary<string, string?>
            {
                ["AWS:AccessKey"] = "test",
                ["AWS:SecretKey"] = "test",
                ["AWS:BucketName"] = "bucket",
                ["AWS:Region"] = "ap-southeast-2",
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            return new AWS(config);
        }

        [Fact]
        public async Task GetInProgressOrder_WhenRepoReturnsNull_Returns404()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out _);
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.GetInProgressOrders(It.IsAny<Guid>())).ReturnsAsync((ViewOrderDTO?)null);

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var result = await sut.GetInProgressOrder(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task StartOrder_WhenAlreadyHasInProgress_Returns400_AndDoesNotStart()
        {
            var repo = new Mock<ITechnicianOrderRepo>(MockBehavior.Strict);
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out _);
            var notificationRepo = new Mock<INotificationRepo>();

            var techId = Guid.NewGuid();
            repo.Setup(r => r.GetInProgressOrders(techId)).ReturnsAsync(new ViewOrderDTO());

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var dto = new OrderActionDTO { OrderId = Guid.NewGuid(), technicianId = techId };

            var result = await sut.StartOrder(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            repo.Verify(r => r.GetInProgressOrders(techId), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task StartOrder_WhenRepoReturnsNull_Returns400()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out _);
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.GetInProgressOrders(It.IsAny<Guid>())).ReturnsAsync((ViewOrderDTO?)null);
            repo.Setup(r => r.StartOrder(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((OrderActionResDTO?)null);

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var result = await sut.StartOrder(new OrderActionDTO { OrderId = Guid.NewGuid(), technicianId = Guid.NewGuid() });

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task StartOrder_WhenNotificationInsertFails_Returns400_AndDoesNotSendHub()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out var clientProxy);
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.GetInProgressOrders(It.IsAny<Guid>())).ReturnsAsync((ViewOrderDTO?)null);
            var actionRes = new OrderActionResDTO
            {
                OrderId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OrderName = "o",
                CreatedAt = DateTime.UtcNow
            };
            repo.Setup(r => r.StartOrder(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(actionRes);
            notificationRepo.Setup(n => n.InsertNewNotification(It.IsAny<InsertNewNotificationDTO>())).ReturnsAsync(false);

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var result = await sut.StartOrder(new OrderActionDTO { OrderId = actionRes.OrderId, technicianId = Guid.NewGuid() });

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            clientProxy.Verify(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task StartOrder_WhenSuccess_InsertsNotification_AndSendsHub_Returns200()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out var clientProxy);
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.GetInProgressOrders(It.IsAny<Guid>())).ReturnsAsync((ViewOrderDTO?)null);
            var actionRes = new OrderActionResDTO
            {
                OrderId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OrderName = "o",
                CreatedAt = DateTime.UtcNow
            };
            repo.Setup(r => r.StartOrder(actionRes.OrderId, It.IsAny<Guid>())).ReturnsAsync(actionRes);
            notificationRepo.Setup(n => n.InsertNewNotification(It.IsAny<InsertNewNotificationDTO>())).ReturnsAsync(true);

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var result = await sut.StartOrder(new OrderActionDTO { OrderId = actionRes.OrderId, technicianId = Guid.NewGuid() });

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            clientProxy.Verify(p => p.SendCoreAsync("ReceiveNotification", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ConfirmOrder_WhenRepoReturnsNull_Returns400()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out _);
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.ConfirmOrder(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((OrderActionResDTO?)null);

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var result = await sut.ConfirmOrder(new OrderActionDTO { OrderId = Guid.NewGuid(), technicianId = Guid.NewGuid() });

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task ConfirmOrder_WhenSuccess_SendsNotification_Returns200()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out var clientProxy);
            var notificationRepo = new Mock<INotificationRepo>();

            var actionRes = new OrderActionResDTO
            {
                OrderId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OrderName = "o",
                CreatedAt = DateTime.UtcNow
            };
            repo.Setup(r => r.ConfirmOrder(actionRes.OrderId, It.IsAny<Guid>())).ReturnsAsync(actionRes);
            notificationRepo.Setup(n => n.InsertNewNotification(It.IsAny<InsertNewNotificationDTO>())).ReturnsAsync(true);

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var result = await sut.ConfirmOrder(new OrderActionDTO { OrderId = actionRes.OrderId, technicianId = Guid.NewGuid() });

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            clientProxy.Verify(p => p.SendCoreAsync("ReceiveNotification", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RejectedOrder_WhenSuccess_Returns200_AndSendsNotification()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out var clientProxy);
            var notificationRepo = new Mock<INotificationRepo>();

            var res = new OrderActionResDTO
            {
                OrderId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                ReceiverId = Guid.NewGuid(),
                OrderName = "o",
                CreatedAt = DateTime.UtcNow
            };
            repo.Setup(r => r.RejectedOrder(res.OrderId, It.IsAny<Guid>())).ReturnsAsync(res);
            notificationRepo.Setup(n => n.InsertNewNotification(It.IsAny<InsertNewNotificationDTO>())).ReturnsAsync(true);

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var result = await sut.RejectedOrder(new OrderActionDTO { OrderId = res.OrderId, technicianId = Guid.NewGuid() });

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            clientProxy.Verify(p => p.SendCoreAsync("ReceiveNotification", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CompleteOrder_WhenRepoReturnsNull_Returns400()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out _);
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.CompletedOrder(It.IsAny<Guid>())).ReturnsAsync((OrderActionResDTO?)null);

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var result = await sut.CompleteOrder(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task GetConfirmingOrders_WhenRepoReturnsNull_ReturnsEmptyList200()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out _);
            var notificationRepo = new Mock<INotificationRepo>();

            var techId = Guid.NewGuid();
            repo.Setup(r => r.GetConfirmingOrders(techId)).ReturnsAsync((List<ViewOrderDTO>?)null);

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var result = await sut.GetConfirmingOrders(techId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetTechnicianLocation_WhenRepoReturnsNull_Returns404()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out _);
            var notificationRepo = new Mock<INotificationRepo>();

            repo.Setup(r => r.GetTechnicianLocation(It.IsAny<Guid>())).ReturnsAsync((GoogleMapDTO?)null);

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws: null!);
            var result = await sut.GetTechnicianLocation(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetOrderDetail_WhenHasVideoAndImages_ConvertsToPublicUrls()
        {
            var repo = new Mock<ITechnicianOrderRepo>();
            var logger = new Mock<ILogger<TechnicianOrderService>>();
            var hub = CreateHub(out _);
            var notificationRepo = new Mock<INotificationRepo>();

            var orderId = Guid.NewGuid();
            var detail = new ViewOrderDetailDTO
            {
                OrderId = orderId,
                videoUrl = "v/key.mp4",
                ImageUrls = new List<string> { "i/1.png", "i/2.png" }
            };

            repo.Setup(r => r.viewOrderDetailDTO(orderId)).ReturnsAsync(detail);

            var aws = CreateAwsForReadOnly();

            var sut = CreateSut(repo, logger, hub, notificationRepo, aws);
            var result = await sut.GetOrderDetail(orderId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("https://", result.Data!.videoUrl);
            Assert.All(result.Data!.ImageUrls, u => Assert.Contains("https://", u));
        }
    }
}
