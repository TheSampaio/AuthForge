using Application.Contracts;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Moq;

namespace AuthForge.Tests.Application
{
    public class ApplicationsServiceTests
    {
        private readonly Mock<IApplicationsRepository> _applicationsRepository = new();
        private readonly Mock<IUserApplicationsRepository> _userApplicationsRepository = new();
        private readonly ApplicationsService _sut;

        public ApplicationsServiceTests()
        {
            _sut = new ApplicationsService(_applicationsRepository.Object, _userApplicationsRepository.Object);
        }

        private static ApplicationsEntity CreateApp(int id = 10, string name = "TodoList") => new()
        {
            Id = id,
            Name = name,
            ClientId = Guid.NewGuid(),
            IsActive = true
        };

        [Fact]
        public async Task CreateApplicationAsync_GrantsCreatorAdminRole()
        {
            var request = new CreateApplicationRequest("TodoList");
            var clientId = Guid.NewGuid();
            var app = new ApplicationsEntity { Id = 5, Name = "TodoList", ClientId = clientId, IsActive = true };

            _applicationsRepository.Setup(r => r.CreateAsync(request.Name, 1)).ReturnsAsync(clientId);
            _applicationsRepository.Setup(r => r.GetByClientIdAsync(clientId)).ReturnsAsync(app);

            var result = await _sut.CreateApplicationAsync(request, requesterUserId: 1);

            Assert.True(result.IsSuccess);
            Assert.Equal(clientId, result.Value);
            _userApplicationsRepository.Verify(r => r.GrantAccessAsync(1, app.Id, "Admin", 1), Times.Once);
        }

        [Fact]
        public async Task AssignUserAsync_SelfAssignUserRole_Succeeds()
        {
            var app = CreateApp();
            var request = new AssignUserRequest(UserId: 2, ClientId: app.ClientId, Role: "User");

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);

            var result = await _sut.AssignUserAsync(request, requesterUserId: 2);

            Assert.True(result.IsSuccess);
            _userApplicationsRepository.Verify(r => r.GrantAccessAsync(2, app.Id, "User", 2), Times.Once);
            _userApplicationsRepository.Verify(r => r.GetGrantAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AssignUserAsync_AdminAssignsOtherUser_Succeeds()
        {
            var app = CreateApp();
            var adminGrant = new UserApplicationsEntity { Id = 1, UserId = 1, ApplicationId = app.Id, Roles = "Admin", IsActive = true };
            var request = new AssignUserRequest(UserId: 2, ClientId: app.ClientId, Role: "Manager");

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(1, app.Id)).ReturnsAsync(adminGrant);

            var result = await _sut.AssignUserAsync(request, requesterUserId: 1);

            Assert.True(result.IsSuccess);
            _userApplicationsRepository.Verify(r => r.GrantAccessAsync(2, app.Id, "Manager", 1), Times.Once);
        }

        [Fact]
        public async Task AssignUserAsync_NonAdminAssignsOtherUser_ReturnsFailure()
        {
            var app = CreateApp();
            var nonAdminGrant = new UserApplicationsEntity { Id = 1, UserId = 1, ApplicationId = app.Id, Roles = "User", IsActive = true };
            var request = new AssignUserRequest(UserId: 2, ClientId: app.ClientId, Role: "Manager");

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(1, app.Id)).ReturnsAsync(nonAdminGrant);

            var result = await _sut.AssignUserAsync(request, requesterUserId: 1);

            Assert.False(result.IsSuccess);
            _userApplicationsRepository.Verify(r => r.GrantAccessAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AssignUserAsync_RevokedAdminGrant_IsTreatedAsNonAdmin()
        {
            var app = CreateApp();
            var revokedAdminGrant = new UserApplicationsEntity { Id = 1, UserId = 1, ApplicationId = app.Id, Roles = "Admin", IsActive = false };
            var request = new AssignUserRequest(UserId: 2, ClientId: app.ClientId, Role: "Manager");

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(1, app.Id)).ReturnsAsync(revokedAdminGrant);

            var result = await _sut.AssignUserAsync(request, requesterUserId: 1);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task AssignUserAsync_ApplicationNotFound_ReturnsFailure()
        {
            var clientId = Guid.NewGuid();
            var request = new AssignUserRequest(UserId: 2, ClientId: clientId, Role: "User");

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(clientId)).ReturnsAsync((ApplicationsEntity?)null);

            var result = await _sut.AssignUserAsync(request, requesterUserId: 1);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task RevokeUserAsync_AdminRevokesOtherUser_Succeeds()
        {
            var app = CreateApp();
            var adminGrant = new UserApplicationsEntity { Id = 1, UserId = 1, ApplicationId = app.Id, Roles = "Admin", IsActive = true };
            var targetGrant = new UserApplicationsEntity { Id = 2, UserId = 2, ApplicationId = app.Id, Roles = "User", IsActive = true };

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(2, app.Id)).ReturnsAsync(targetGrant);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(1, app.Id)).ReturnsAsync(adminGrant);

            var result = await _sut.RevokeUserAsync(app.ClientId, userId: 2, requesterUserId: 1);

            Assert.True(result.IsSuccess);
            _userApplicationsRepository.Verify(r => r.RevokeAccessAsync(2, app.Id, 1), Times.Once);
        }

        [Fact]
        public async Task RevokeUserAsync_SelfRevoke_Succeeds()
        {
            var app = CreateApp();
            var ownGrant = new UserApplicationsEntity { Id = 1, UserId = 1, ApplicationId = app.Id, Roles = "User", IsActive = true };

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(1, app.Id)).ReturnsAsync(ownGrant);

            var result = await _sut.RevokeUserAsync(app.ClientId, userId: 1, requesterUserId: 1);

            Assert.True(result.IsSuccess);
            _userApplicationsRepository.Verify(r => r.RevokeAccessAsync(1, app.Id, 1), Times.Once);
        }

        [Fact]
        public async Task RevokeUserAsync_NonAdminRevokesOtherUser_ReturnsFailure()
        {
            var app = CreateApp();
            var nonAdminGrant = new UserApplicationsEntity { Id = 1, UserId = 1, ApplicationId = app.Id, Roles = "User", IsActive = true };
            var targetGrant = new UserApplicationsEntity { Id = 2, UserId = 2, ApplicationId = app.Id, Roles = "User", IsActive = true };

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(2, app.Id)).ReturnsAsync(targetGrant);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(1, app.Id)).ReturnsAsync(nonAdminGrant);

            var result = await _sut.RevokeUserAsync(app.ClientId, userId: 2, requesterUserId: 1);

            Assert.False(result.IsSuccess);
            _userApplicationsRepository.Verify(r => r.RevokeAccessAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task RevokeUserAsync_TargetUserHasNoActiveGrant_ReturnsFailure()
        {
            var app = CreateApp();

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(2, app.Id)).ReturnsAsync((UserApplicationsEntity?)null);

            var result = await _sut.RevokeUserAsync(app.ClientId, userId: 2, requesterUserId: 1);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DeactivateApplicationAsync_Admin_Succeeds()
        {
            var app = CreateApp();
            var adminGrant = new UserApplicationsEntity { Id = 1, UserId = 1, ApplicationId = app.Id, Roles = "Admin", IsActive = true };

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(1, app.Id)).ReturnsAsync(adminGrant);

            var result = await _sut.DeactivateApplicationAsync(app.ClientId, requesterUserId: 1);

            Assert.True(result.IsSuccess);
            _applicationsRepository.Verify(r => r.DeactivateAsync(app, 1), Times.Once);
        }

        [Fact]
        public async Task DeactivateApplicationAsync_NonAdmin_ReturnsFailure()
        {
            var app = CreateApp();
            var userGrant = new UserApplicationsEntity { Id = 1, UserId = 1, ApplicationId = app.Id, Roles = "User", IsActive = true };

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(1, app.Id)).ReturnsAsync(userGrant);

            var result = await _sut.DeactivateApplicationAsync(app.ClientId, requesterUserId: 1);

            Assert.False(result.IsSuccess);
            _applicationsRepository.Verify(r => r.DeactivateAsync(It.IsAny<ApplicationsEntity>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetUserApplicationsAsync_ReturnsMappedResponses()
        {
            var apps = new[]
            {
                new ApplicationsEntity { Id = 1, Name = "TodoList", ClientId = Guid.NewGuid(), IsActive = true },
                new ApplicationsEntity { Id = 2, Name = "TicketManager", ClientId = Guid.NewGuid(), IsActive = true }
            };

            _applicationsRepository.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(apps);

            var result = await _sut.GetUserApplicationsAsync(1);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.Count());
            Assert.Contains(result.Value!, a => a.Name == "TodoList" && a.ClientId == apps[0].ClientId);
        }
    }
}