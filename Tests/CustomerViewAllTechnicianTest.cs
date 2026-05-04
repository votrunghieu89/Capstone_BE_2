using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Customer.FindTechnician;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Repositories.Customer;
using Capstone_2_BE.Repositories.Technician;
using Capstone_2_BE.Services.Customer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class CustomerViewAllTechnicianTest
    {
        private static CustomerViewAllTechnicianService CreateSut(
            Mock<ICustomerViewAllTechnicianRepo> repo,
            Mock<ILogger<CustomerViewAllTechnicianService>> logger,
            Mock<IServiceRepo> serviceRepo,
            Mock<ITechnicianProfileRepo> techRepo)
            => new CustomerViewAllTechnicianService(
                hubContext: Mock.Of<IHubContext<Capstone_2_BE.Socket.NotificationHub>>(),
                notificationRepo: Mock.Of<Capstone_2_BE.Repositories.INotificationRepo>(),
                repo: repo.Object,
                aws: null!,
                logger: logger.Object,
                aIEstimationTime: null!,
                serviceDAL: serviceRepo.Object,
                technicianProfileDAL: techRepo.Object);

        [Fact]
        public async Task FilterTechnicianCombination_WhenFilterNull_Returns400()
        {
            var repo = new Mock<ICustomerViewAllTechnicianRepo>(MockBehavior.Strict);
            var logger = new Mock<ILogger<CustomerViewAllTechnicianService>>();
            var serviceRepo = new Mock<IServiceRepo>();
            var techRepo = new Mock<ITechnicianProfileRepo>();

            var sut = CreateSut(repo, logger, serviceRepo, techRepo);
            var result = await sut.FilterTechnicianCombination(null);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task FilterTechnicianCombination_WhenRatingInvalid_Returns400_AndDoesNotCallRepo()
        {
            var repo = new Mock<ICustomerViewAllTechnicianRepo>(MockBehavior.Strict);
            var logger = new Mock<ILogger<CustomerViewAllTechnicianService>>();
            var serviceRepo = new Mock<IServiceRepo>();
            var techRepo = new Mock<ITechnicianProfileRepo>();

            var sut = CreateSut(repo, logger, serviceRepo, techRepo);
            var filter = new TechnicianFilterRequestDTO { startRate = -1 };

            var result = await sut.FilterTechnicianCombination(filter);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            repo.VerifyAll();
        }

        [Fact]
        public async Task ViewAllTechnician_WhenRepoReturnsNull_ReturnsFailure500()
        {
            var repo = new Mock<ICustomerViewAllTechnicianRepo>();
            var logger = new Mock<ILogger<CustomerViewAllTechnicianService>>();
            var serviceRepo = new Mock<IServiceRepo>();
            var techRepo = new Mock<ITechnicianProfileRepo>();

            repo.Setup(r => r.ViewALLTechnician()).ReturnsAsync((List<ViewAllTechnicianDTO>?)null);

            var sut = CreateSut(repo, logger, serviceRepo, techRepo);
            var result = await sut.ViewAllTechnician();

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task FilterByArea_WhenRepoReturnsEmpty_Returns200EmptyList()
        {
            var repo = new Mock<ICustomerViewAllTechnicianRepo>();
            var logger = new Mock<ILogger<CustomerViewAllTechnicianService>>();
            var serviceRepo = new Mock<IServiceRepo>();
            var techRepo = new Mock<ITechnicianProfileRepo>();

            repo.Setup(r => r.FilterTechnicianbyArea(It.IsAny<Guid>())).ReturnsAsync(new List<ViewAllTechnicianDTO>());

            var sut = CreateSut(repo, logger, serviceRepo, techRepo);
            var result = await sut.FilterByArea(Guid.NewGuid());

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task FilterByService_WhenRepoReturnsList_Returns200AndSameList()
        {
            var repo = new Mock<ICustomerViewAllTechnicianRepo>();
            var logger = new Mock<ILogger<CustomerViewAllTechnicianService>>();
            var serviceRepo = new Mock<IServiceRepo>();
            var techRepo = new Mock<ITechnicianProfileRepo>();

            var list = new List<ViewAllTechnicianDTO> { new ViewAllTechnicianDTO { TechnicianId = Guid.NewGuid(), TechnicianName = "T" } };
            repo.Setup(r => r.FilterTechnicianbyService(It.IsAny<Guid>())).ReturnsAsync(list);

            var sut = CreateSut(repo, logger, serviceRepo, techRepo);
            var result = await sut.FilterByService(Guid.NewGuid());

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(list, result.Data);
        }

        [Fact]
        public async Task FilterByRate_WhenRepoReturnsList_Returns200()
        {
            var repo = new Mock<ICustomerViewAllTechnicianRepo>();
            var logger = new Mock<ILogger<CustomerViewAllTechnicianService>>();
            var serviceRepo = new Mock<IServiceRepo>();
            var techRepo = new Mock<ITechnicianProfileRepo>();

            var list = new List<ViewAllTechnicianDTO> { new ViewAllTechnicianDTO { TechnicianId = Guid.NewGuid(), TechnicianName = "T" } };
            repo.Setup(r => r.FilterTechnicianbyRate(3, 5)).ReturnsAsync(list);

            var sut = CreateSut(repo, logger, serviceRepo, techRepo);
            var result = await sut.FilterByRate(3, 5);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(list, result.Data);
        }

        [Fact]
        public async Task SearchByName_WhenRepoReturnsNull_Returns200EmptyList()
        {
            var repo = new Mock<ICustomerViewAllTechnicianRepo>();
            var logger = new Mock<ILogger<CustomerViewAllTechnicianService>>();
            var serviceRepo = new Mock<IServiceRepo>();
            var techRepo = new Mock<ITechnicianProfileRepo>();

            repo.Setup(r => r.SearchTechnicianbyName("a")).ReturnsAsync((List<ViewAllTechnicianDTO>?)null);

            var sut = CreateSut(repo, logger, serviceRepo, techRepo);
            var result = await sut.SearchByName("a");

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task ViewDetailOfTechnician_WhenRepoReturnsNull_Returns404()
        {
            var repo = new Mock<ICustomerViewAllTechnicianRepo>();
            var logger = new Mock<ILogger<CustomerViewAllTechnicianService>>();
            var serviceRepo = new Mock<IServiceRepo>();
            var techRepo = new Mock<ITechnicianProfileRepo>();

            repo.Setup(r => r.ViewDetailOfTechnician(It.IsAny<Guid>())).ReturnsAsync((TechnicianDetailDTO?)null);

            var sut = CreateSut(repo, logger, serviceRepo, techRepo);
            var result = await sut.ViewDetailOfTechnician(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }
    }
}
