using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Technician.Rating;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Services.Technician;
using Capstone_2_BE.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class TechnicianRatingTest
    {
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

        private static TechnicianRatingService CreateSut(Mock<ITechnicianRatingRepo> repo, Mock<ILogger<TechnicianRatingService>> logger, AWS aws)
            => new TechnicianRatingService(repo.Object, aws, logger.Object);

        [Fact]
        public async Task GetTechnicianRatingOverview_WhenRepoReturnsNull_Returns404()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.getTechniqueRateOverview(id)).ReturnsAsync((TechnicianRatingViewDTO?)null);

            var sut = CreateSut(repo, logger, aws: CreateAwsForReadOnly());
            var result = await sut.GetTechnicianRatingOverview(id);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetTechnicianRatingOverview_WhenRepoReturnsDtoWithAvatar_ConvertsAvatar_AndReturns200()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            var id = Guid.NewGuid();
            var dto = new TechnicianRatingViewDTO { AvatarURL = "profile/a.png" };
            repo.Setup(r => r.getTechniqueRateOverview(id)).ReturnsAsync(dto);

            var sut = CreateSut(repo, logger, aws: CreateAwsForReadOnly());
            var result = await sut.GetTechnicianRatingOverview(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.Contains("https://", result.Data!.AvatarURL);
        }

        [Fact]
        public async Task GetTechnicianRatingOverview_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            repo.Setup(r => r.getTechniqueRateOverview(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger, aws: CreateAwsForReadOnly());
            var result = await sut.GetTechnicianRatingOverview(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task GetTechnicianFeedbacks_WhenRepoReturnsEmpty_ReturnsEmptyList200()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.getTechniqueFeedBack(id)).ReturnsAsync(new List<TechnicianFeedbackViewDTO>());

            var sut = CreateSut(repo, logger, aws: CreateAwsForReadOnly());
            var result = await sut.GetTechnicianFeedbacks(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetTechnicianFeedbacks_WhenRepoReturnsNull_ReturnsEmptyList200()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            repo.Setup(r => r.getTechniqueFeedBack(It.IsAny<Guid>())).ReturnsAsync((List<TechnicianFeedbackViewDTO>?)null);

            var sut = CreateSut(repo, logger, aws: CreateAwsForReadOnly());
            var result = await sut.GetTechnicianFeedbacks(Guid.NewGuid());

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetTechnicianFeedbacks_WhenRepoReturnsItemsWithAvatar_ConvertsEachAvatar()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            var techId = Guid.NewGuid();
            var feedbacks = new List<TechnicianFeedbackViewDTO>
            {
                new TechnicianFeedbackViewDTO { CustomerAvatarURL = "c/1.png" },
                new TechnicianFeedbackViewDTO { CustomerAvatarURL = null },
                new TechnicianFeedbackViewDTO { CustomerAvatarURL = "c/2.png" },
            };

            repo.Setup(r => r.getTechniqueFeedBack(techId)).ReturnsAsync(feedbacks);

            var sut = CreateSut(repo, logger, aws: CreateAwsForReadOnly());
            var result = await sut.GetTechnicianFeedbacks(techId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("https://", result.Data![0].CustomerAvatarURL);
            Assert.Null(result.Data![1].CustomerAvatarURL);
            Assert.Contains("https://", result.Data![2].CustomerAvatarURL);
        }

        [Fact]
        public async Task GetTechnicianFeedbacks_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            repo.Setup(r => r.getTechniqueFeedBack(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger, aws: CreateAwsForReadOnly());
            var result = await sut.GetTechnicianFeedbacks(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public async Task GetDetailOrderofFeedback_WhenRepoReturnsNull_Returns404()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            repo.Setup(r => r.getDetailOrderofFeedback(It.IsAny<Guid>())).ReturnsAsync((TechnicianGetOrderFromFeedbackDTO?)null);

            var sut = CreateSut(repo, logger, aws: CreateAwsForReadOnly());
            var result = await sut.GetDetailOrderofFeedback(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetDetailOrderofFeedback_WhenRepoReturnsDto_Returns200AndSameDto()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            var feedbackId = Guid.NewGuid();
            var dto = new TechnicianGetOrderFromFeedbackDTO();
            repo.Setup(r => r.getDetailOrderofFeedback(feedbackId)).ReturnsAsync(dto);

            var sut = CreateSut(repo, logger, aws: CreateAwsForReadOnly());
            var result = await sut.GetDetailOrderofFeedback(feedbackId);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Same(dto, result.Data);
        }

        [Fact]
        public async Task GetDetailOrderofFeedback_WhenRepoThrows_Returns500()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            repo.Setup(r => r.getDetailOrderofFeedback(It.IsAny<Guid>())).ThrowsAsync(new Exception("boom"));

            var sut = CreateSut(repo, logger, aws: CreateAwsForReadOnly());
            var result = await sut.GetDetailOrderofFeedback(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(500, result.StatusCode);
        }
    }
}
