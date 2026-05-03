using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Customer.Rating;
using Capstone_2_BE.Repositories.Customer;
using Capstone_2_BE.Services.Customer;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class CustomerRatingTest
    {
        private static CustomerRatingService CreateSut(Mock<ICustomerRatingRepo> repo, Mock<ILogger<CustomerRatingService>> logger)
            => new CustomerRatingService(repo.Object, logger.Object);

        [Fact]
        public async Task CreateFeedBack_WhenRepoReturnsTrue_Returns200()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            var dto = new CreateFeedbackDTO { CustomerId = Guid.NewGuid(), TechnicianId = Guid.NewGuid(), OrderId = Guid.NewGuid(), Score = 5, Feedback = "ok" };
            repo.Setup(r => r.CreateFeedBack(dto)).ReturnsAsync(true);

            var result = await sut.CreateFeedBack(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task CreateFeedBack_WhenRepoReturnsFalse_Returns400()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            var dto = new CreateFeedbackDTO { CustomerId = Guid.NewGuid(), TechnicianId = Guid.NewGuid(), OrderId = Guid.NewGuid(), Score = 1, Feedback = "x" };
            repo.Setup(r => r.CreateFeedBack(dto)).ReturnsAsync(false);

            var result = await sut.CreateFeedBack(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreateFeedBack_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            repo.Setup(r => r.CreateFeedBack(It.IsAny<CreateFeedbackDTO>())).ThrowsAsync(new Exception("boom"));

            var result = await sut.CreateFeedBack(new CreateFeedbackDTO { CustomerId = Guid.NewGuid(), TechnicianId = Guid.NewGuid(), OrderId = Guid.NewGuid(), Score = 5, Feedback = "ok" });

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task IsFeedback_WhenOrderIdEmpty_Returns400_AndDoesNotCallRepo()
        {
            var repo = new Mock<ICustomerRatingRepo>(MockBehavior.Strict);
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            var result = await sut.IsFeedback(Guid.Empty);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("OrderId không hợp lệ", result.Error);
            repo.VerifyAll();
        }

        [Fact]
        public async Task IsFeedback_WhenRepoReturnsTrue_Returns200AndTrue()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            var orderId = Guid.NewGuid();
            repo.Setup(r => r.IsFeedback(orderId)).ReturnsAsync(true);

            var result = await sut.IsFeedback(orderId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task IsFeedback_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            repo.Setup(r => r.IsFeedback(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var result = await sut.IsFeedback(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Lỗi hệ thống", result.Error);
        }

        [Fact]
        public async Task ViewCreatedFeedBack_WhenRepoReturnsNull_Returns200EmptyList()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            repo.Setup(r => r.ViewCreatedFeedBack(It.IsAny<Guid>())).ReturnsAsync((List<ViewFeedBackDTO>?)null);

            var result = await sut.ViewCreatedFeedBack(Guid.NewGuid());

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task ViewCreatedFeedBack_WhenRepoReturnsList_Returns200AndSameList()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            var list = new List<ViewFeedBackDTO> { new ViewFeedBackDTO { FeedbackId = Guid.NewGuid(), OrderId = Guid.NewGuid(), TechnicianId = Guid.NewGuid(), Score = 5, Feedback = "ok" } };
            repo.Setup(r => r.ViewCreatedFeedBack(It.IsAny<Guid>())).ReturnsAsync(list);

            var result = await sut.ViewCreatedFeedBack(Guid.NewGuid());

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(list, result.Data);
        }

        [Fact]
        public async Task ViewCreatedFeedBack_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            repo.Setup(r => r.ViewCreatedFeedBack(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var result = await sut.ViewCreatedFeedBack(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task UpdateFeedBack_WhenRepoReturnsTrue_Returns200()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            var dto = new UpdateFeedbackDTO { FeedbackId = Guid.NewGuid(), Score = 3, Feedback = "x" };
            repo.Setup(r => r.UpdateFeedBack(dto)).ReturnsAsync(true);

            var result = await sut.UpdateFeedBack(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task UpdateFeedBack_WhenRepoReturnsFalse_Returns400()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            var dto = new UpdateFeedbackDTO { FeedbackId = Guid.NewGuid(), Score = 3, Feedback = "x" };
            repo.Setup(r => r.UpdateFeedBack(dto)).ReturnsAsync(false);

            var result = await sut.UpdateFeedBack(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UpdateFeedBack_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            repo.Setup(r => r.UpdateFeedBack(It.IsAny<UpdateFeedbackDTO>())).ThrowsAsync(new Exception("boom"));

            var result = await sut.UpdateFeedBack(new UpdateFeedbackDTO { FeedbackId = Guid.NewGuid(), Score = 3, Feedback = "x" });

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task DeleteFeedBack_WhenRepoReturnsTrue_Returns200()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            var id = Guid.NewGuid();
            repo.Setup(r => r.DeleteFeedBack(id)).ReturnsAsync(true);

            var result = await sut.DeleteFeedBack(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task DeleteFeedBack_WhenRepoReturnsFalse_Returns400()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            var id = Guid.NewGuid();
            repo.Setup(r => r.DeleteFeedBack(id)).ReturnsAsync(false);

            var result = await sut.DeleteFeedBack(id);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task DeleteFeedBack_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ICustomerRatingRepo>();
            var logger = new Mock<ILogger<CustomerRatingService>>();
            var sut = CreateSut(repo, logger);

            repo.Setup(r => r.DeleteFeedBack(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var result = await sut.DeleteFeedBack(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
    }
}
