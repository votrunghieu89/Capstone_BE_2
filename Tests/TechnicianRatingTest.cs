using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Technician.Rating;
using Capstone_2_BE.Repositories;
using Capstone_2_BE.Services.Technician;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class TechnicianRatingTest
    {
        [Fact]
        public async Task GetTechnicianRatingOverview_WhenRepoReturnsNull_Returns404()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.getTechniqueRateOverview(id)).ReturnsAsync((TechnicianRatingViewDTO?)null);

            var sut = new TechnicianRatingService(repo.Object, aws: null!, logger.Object);
            var result = await sut.GetTechnicianRatingOverview(id);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task GetTechnicianFeedbacks_WhenRepoReturnsEmpty_ReturnsEmptyList200()
        {
            var repo = new Mock<ITechnicianRatingRepo>();
            var logger = new Mock<ILogger<TechnicianRatingService>>();

            var id = Guid.NewGuid();
            repo.Setup(r => r.getTechniqueFeedBack(id)).ReturnsAsync(new List<TechnicianFeedbackViewDTO>());

            var sut = new TechnicianRatingService(repo.Object, aws: null!, logger.Object);
            var result = await sut.GetTechnicianFeedbacks(id);

            Assert.True(result.IsSuccess);
            Assert.Equal(200, result.StatusCode);
            Assert.Empty(result.Data);
        }
    }
}
