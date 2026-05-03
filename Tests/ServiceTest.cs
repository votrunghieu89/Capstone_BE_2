using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Service;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class ServiceTest
    {
        private static ServiceType CreateSut(Mock<IServiceRepo> repo, Mock<ILogger<ServiceType>> logger)
            => new ServiceType(repo.Object, logger.Object);

        [Fact]
        public async Task GetServiceName_WhenRepoReturnsNullOrEmpty_Returns404()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();
            var serviceId = Guid.NewGuid();

            repo.Setup(r => r.GetServiceName(serviceId)).ReturnsAsync((string?)null);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetServiceName(serviceId);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Service not found", result.Error);
        }

        [Fact]
        public async Task GetServiceName_WhenRepoReturnsName_Returns200AndName()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();
            var serviceId = Guid.NewGuid();

            repo.Setup(r => r.GetServiceName(serviceId)).ReturnsAsync("Electrical");

            var sut = CreateSut(repo, logger);
            var result = await sut.GetServiceName(serviceId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Electrical", result.Data);
        }

        [Fact]
        public async Task GetServiceName_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            repo.Setup(r => r.GetServiceName(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.GetServiceName(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error retrieving service name", result.Error);
        }

        [Fact]
        public async Task GetAllServices_WhenRepoReturnsList_Returns200AndSameList()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var list = new List<ServiceDTO> { new ServiceDTO { Id = Guid.NewGuid(), ServiceName = "S", Description = "D" } };
            repo.Setup(r => r.GetAllServices()).ReturnsAsync(list);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetAllServices();

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(list, result.Data);
        }

        [Fact]
        public async Task GetAllServices_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            repo.Setup(r => r.GetAllServices()).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.GetAllServices();

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error retrieving services", result.Error);
        }

        [Fact]
        public async Task GetServiceIdByName_WhenRepoReturnsNull_Returns404()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            repo.Setup(r => r.GetServiceIdByName("Plumbing")).ReturnsAsync((Guid?)null);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetServiceIdByName("Plumbing");

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Service not found", result.Error);
        }

        [Fact]
        public async Task GetServiceIdByName_WhenRepoReturnsGuid_Returns200AndGuid()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.GetServiceIdByName("Plumbing")).ReturnsAsync(id);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetServiceIdByName("Plumbing");

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(id, result.Data);
        }

        [Fact]
        public async Task AddService_WhenRepoReturnsNull_Returns400()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var dto = new CreateServiceAdminDTO { ServiceName = "S1", Description = "D" };
            repo.Setup(r => r.AddService(dto)).ReturnsAsync((Guid?)null);

            var sut = CreateSut(repo, logger);
            var result = await sut.AddService(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Cannot add service", result.Error);
        }

        [Fact]
        public async Task AddService_WhenRepoReturnsGuid_Returns201AndGuid()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var dto = new CreateServiceAdminDTO { ServiceName = "S1", Description = "D" };
            var id = Guid.NewGuid();
            repo.Setup(r => r.AddService(dto)).ReturnsAsync(id);

            var sut = CreateSut(repo, logger);
            var result = await sut.AddService(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(id, result.Data);
        }

        [Fact]
        public async Task AddService_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var dto = new CreateServiceAdminDTO { ServiceName = "S1", Description = "D" };
            repo.Setup(r => r.AddService(dto)).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.AddService(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error adding service", result.Error);
        }

        [Fact]
        public async Task GetAllServicesAdmin_WhenRepoReturnsList_Returns200AndSameList()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var list = new List<ServiceAdminDTO> { new ServiceAdminDTO { Id = Guid.NewGuid(), ServiceName = "S", Description = "D" } };
            repo.Setup(r => r.GetAllServicesAdmin()).ReturnsAsync(list);

            var sut = CreateSut(repo, logger);
            var result = await sut.GetAllServicesAdmin();

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(list, result.Data);
        }

        [Fact]
        public async Task GetAllServicesAdmin_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            repo.Setup(r => r.GetAllServicesAdmin()).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.GetAllServicesAdmin();

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error retrieving services", result.Error);
        }

        [Fact]
        public async Task UpdateService_WhenRepoReturnsTrue_Returns200()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var dto = new ServiceDTO { Id = Guid.NewGuid(), ServiceName = "S1", Description = "D" };
            repo.Setup(r => r.UpdateService(dto)).ReturnsAsync(true);

            var sut = CreateSut(repo, logger);
            var result = await sut.UpdateService(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Cập nhật dịch vụ thành công", result.Data);
        }

        [Fact]
        public async Task UpdateService_WhenRepoReturnsFalse_Returns400()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var dto = new ServiceDTO { Id = Guid.NewGuid(), ServiceName = "S1", Description = "D" };
            repo.Setup(r => r.UpdateService(dto)).ReturnsAsync(false);

            var sut = CreateSut(repo, logger);
            var result = await sut.UpdateService(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Cập nhật dịch vụ thất bại", result.Error);
        }

        [Fact]
        public async Task UpdateService_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var dto = new ServiceDTO { Id = Guid.NewGuid(), ServiceName = "S1", Description = "D" };
            repo.Setup(r => r.UpdateService(dto)).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.UpdateService(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Lỗi khi cập nhật dịch vụ", result.Error);
        }

        [Fact]
        public async Task DeleteService_WhenRepoReturnsTrue_Returns200()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.DeleteService(id)).ReturnsAsync(true);

            var sut = CreateSut(repo, logger);
            var result = await sut.DeleteService(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Xóa dịch vụ thành công", result.Data);
        }

        [Fact]
        public async Task DeleteService_WhenRepoReturnsFalse_Returns400()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.DeleteService(id)).ReturnsAsync(false);

            var sut = CreateSut(repo, logger);
            var result = await sut.DeleteService(id);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Xóa dịch vụ thất bại", result.Error);
        }

        [Fact]
        public async Task DeleteService_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<IServiceRepo>();
            var logger = new Mock<ILogger<ServiceType>>();

            repo.Setup(r => r.DeleteService(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger);
            var result = await sut.DeleteService(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Lỗi khi xóa dịch vụ", result.Error);
        }
    }
}
