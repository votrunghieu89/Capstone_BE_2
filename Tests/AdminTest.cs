using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capstone_2_BE.DTOs.Admin;
using Capstone_2_BE.Repositories.Admin;
using Capstone_2_BE.Services.Admin;
using Moq;
using Xunit;

namespace Capstone_2_BE.Tests
{
    public class AdminTest
    {
        [Fact]
        public async Task GetUsers_ReturnsRepoData()
        {
            var repo = new Mock<IAdminRepo>();
            var data = new List<object> { new { id = Guid.NewGuid(), email = "a@b.com" } };
            repo.Setup(r => r.GetUsers()).ReturnsAsync(data);

            var sut = new AdminService(repo.Object);
            var result = await sut.GetUsers();

            Assert.True(result.IsSuccess);
            Assert.Same(data, result.Data);
        }

        [Fact]
        public async Task GetRequests_ReturnsRepoData()
        {
            var repo = new Mock<IAdminRepo>();
            var data = new List<object> { new { id = Guid.NewGuid(), title = "t" } };
            repo.Setup(r => r.GetRequests()).ReturnsAsync(data);

            var sut = new AdminService(repo.Object);
            var result = await sut.GetRequests();

            Assert.True(result.IsSuccess);
            Assert.Same(data, result.Data);
        }

        [Fact]
        public async Task GetDashboardStats_ReturnsRepoData()
        {
            var repo = new Mock<IAdminRepo>();
            var data = new { summary = new { totalUsers = 1 } };
            repo.Setup(r => r.GetDashboardStats()).ReturnsAsync(data);

            var sut = new AdminService(repo.Object);
            var result = await sut.GetDashboardStats();

            Assert.True(result.IsSuccess);
            Assert.Same(data, result.Data);
        }

        [Fact]
        public async Task GetFeedback_ReturnsRepoData()
        {
            var repo = new Mock<IAdminRepo>();
            var data = new List<object> { new { id = Guid.NewGuid(), score = 5 } };
            repo.Setup(r => r.GetFeedback()).ReturnsAsync(data);

            var sut = new AdminService(repo.Object);
            var result = await sut.GetFeedback();

            Assert.True(result.IsSuccess);
            Assert.Same(data, result.Data);
        }

        [Theory]
        [InlineData(true, true, 200, "Locked")]
        [InlineData(false, false, 404, "User not found")]
        public async Task LockUser_MapsRepoResult(bool repoOk, bool expectSuccess, int status, string msg)
        {
            var repo = new Mock<IAdminRepo>();
            var id = Guid.NewGuid();
            repo.Setup(r => r.UpdateUserStatus(id, 0)).ReturnsAsync(repoOk);

            var sut = new AdminService(repo.Object);
            var result = await sut.LockUser(id);

            Assert.Equal(expectSuccess, result.IsSuccess);
            Assert.Equal(status, result.StatusCode);
            Assert.Equal(msg, expectSuccess ? result.Data : result.Error);
        }

        [Theory]
        [InlineData(true, true, 200, "Unlocked")]
        [InlineData(false, false, 404, "User not found")]
        public async Task UnlockUser_MapsRepoResult(bool repoOk, bool expectSuccess, int status, string msg)
        {
            var repo = new Mock<IAdminRepo>();
            var id = Guid.NewGuid();
            repo.Setup(r => r.UpdateUserStatus(id, 1)).ReturnsAsync(repoOk);

            var sut = new AdminService(repo.Object);
            var result = await sut.UnlockUser(id);

            Assert.Equal(expectSuccess, result.IsSuccess);
            Assert.Equal(status, result.StatusCode);
            Assert.Equal(msg, expectSuccess ? result.Data : result.Error);
        }

        [Theory]
        [InlineData(true, true, 200, "Deleted")]
        [InlineData(false, false, 404, "Not found")]
        public async Task DeleteFeedback_MapsRepoResult(bool repoOk, bool expectSuccess, int status, string msg)
        {
            var repo = new Mock<IAdminRepo>();
            var id = Guid.NewGuid();
            repo.Setup(r => r.DeleteFeedback(id)).ReturnsAsync(repoOk);

            var sut = new AdminService(repo.Object);
            var result = await sut.DeleteFeedback(id);

            Assert.Equal(expectSuccess, result.IsSuccess);
            Assert.Equal(status, result.StatusCode);
            Assert.Equal(msg, expectSuccess ? result.Data : result.Error);
        }

        [Fact]
        public async Task GetTechniciansFull_ReturnsRepoData()
        {
            var repo = new Mock<IAdminRepo>();
            var data = new List<object> { new { id = Guid.NewGuid(), name = "t" } };
            repo.Setup(r => r.GetTechniciansFull()).ReturnsAsync(data);

            var sut = new AdminService(repo.Object);
            var result = await sut.GetTechniciansFull();

            Assert.True(result.IsSuccess);
            Assert.Same(data, result.Data);
        }

        [Fact]
        public async Task GetTechnicianReviews_ReturnsRepoData()
        {
            var repo = new Mock<IAdminRepo>();
            var techId = Guid.NewGuid();
            var data = new List<object> { new { ratingId = Guid.NewGuid(), score = 5 } };
            repo.Setup(r => r.GetTechnicianReviews(techId)).ReturnsAsync(data);

            var sut = new AdminService(repo.Object);
            var result = await sut.GetTechnicianReviews(techId);

            Assert.True(result.IsSuccess);
            Assert.Same(data, result.Data);
        }

        [Fact]
        public async Task CreateTechnician_ReturnsRepoData()
        {
            var repo = new Mock<IAdminRepo>();
            var dto = new CreateTechnicianDto { Email = "t@ex.com", FullName = "T", PhoneNumber = "1" };
            var data = new { id = Guid.NewGuid(), name = "T" };

            repo.Setup(r => r.CreateTechnician(dto)).ReturnsAsync(data);

            var sut = new AdminService(repo.Object);
            var result = await sut.CreateTechnician(dto);

            Assert.True(result.IsSuccess);
            Assert.Same(data, result.Data);
        }
    }
}
