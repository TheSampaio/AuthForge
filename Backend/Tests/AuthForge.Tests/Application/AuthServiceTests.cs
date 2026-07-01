using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using AuthForge.Tests.TestHelpers;
using Domain.Entities;
using Domain.Interfaces;
using Moq;

namespace AuthForge.Tests.Application
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsersRepository> _usersRepository = new();
        private readonly Mock<IApplicationsRepository> _applicationsRepository = new();
        private readonly Mock<IUserApplicationsRepository> _userApplicationsRepository = new();
        private readonly Mock<ICryptoService> _cryptoService = new();
        private readonly Mock<IJwtService> _jwtService = new();
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _sut = new AuthService(
                _usersRepository.Object,
                _applicationsRepository.Object,
                _userApplicationsRepository.Object,
                _cryptoService.Object,
                _jwtService.Object,
                ConfigurationTestHelper.BuildJwtConfiguration());
        }

        private static UsersEntity CreateUser(int id = 1, string email = "user@test.com", string passwordHash = "hashed") => new()
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            PasswordHash = passwordHash,
            Birthdate = new DateTime(1990, 1, 1)
        };

        private static ApplicationsEntity CreateApp(int id = 10, string name = "TodoList", bool isActive = true) => new()
        {
            Id = id,
            Name = name,
            ClientId = Guid.NewGuid(),
            IsActive = isActive
        };

        // ---- LoginAsync ----

        [Fact]
        public async Task LoginAsync_WithValidCredentialsAndActiveGrant_ReturnsToken()
        {
            var user = CreateUser();
            var app = CreateApp();
            var grant = new UserApplicationsEntity { Id = 1, UserId = user.Id, ApplicationId = app.Id, Roles = "User", IsActive = true };
            var request = new SsoLoginRequest(user.Email, "password", app.ClientId);

            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(user.Id, app.Id)).ReturnsAsync(grant);
            _jwtService.Setup(j => j.GenerateToken(user, app.Name, grant.Roles)).Returns("sso-token");

            var result = await _sut.LoginAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal("sso-token", result.Value!.Token);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
        {
            var user = CreateUser();
            var request = new SsoLoginRequest(user.Email, "wrong-password", Guid.NewGuid());

            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(false);

            var result = await _sut.LoginAsync(request);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task LoginAsync_WithUnknownEmail_ReturnsFailure()
        {
            var request = new SsoLoginRequest("ghost@test.com", "whatever", Guid.NewGuid());

            _usersRepository.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((UsersEntity?)null);

            var result = await _sut.LoginAsync(request);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task LoginAsync_WithInactiveApplication_ReturnsFailure()
        {
            var user = CreateUser();
            var app = CreateApp(isActive: false);
            var request = new SsoLoginRequest(user.Email, "password", app.ClientId);

            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);

            var result = await _sut.LoginAsync(request);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task LoginAsync_WithUnknownApplication_ReturnsFailure()
        {
            var user = CreateUser();
            var clientId = Guid.NewGuid();
            var request = new SsoLoginRequest(user.Email, "password", clientId);

            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
            _applicationsRepository.Setup(r => r.GetByClientIdAsync(clientId)).ReturnsAsync((ApplicationsEntity?)null);

            var result = await _sut.LoginAsync(request);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task LoginAsync_WithNoGrant_ReturnsFailure()
        {
            var user = CreateUser();
            var app = CreateApp();
            var request = new SsoLoginRequest(user.Email, "password", app.ClientId);

            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(user.Id, app.Id)).ReturnsAsync((UserApplicationsEntity?)null);

            var result = await _sut.LoginAsync(request);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task LoginAsync_WithRevokedGrant_ReturnsFailure()
        {
            var user = CreateUser();
            var app = CreateApp();
            var grant = new UserApplicationsEntity { Id = 1, UserId = user.Id, ApplicationId = app.Id, Roles = "User", IsActive = false };
            var request = new SsoLoginRequest(user.Email, "password", app.ClientId);

            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(user.Id, app.Id)).ReturnsAsync(grant);

            var result = await _sut.LoginAsync(request);

            Assert.False(result.IsSuccess);
        }

        // ---- RegisterAsync ----

        [Fact]
        public async Task RegisterAsync_NewUser_CreatesUserGrantsAccessAndReturnsToken()
        {
            var app = CreateApp();
            var request = new SsoRegisterRequest("Jane", "Doe", "jane@test.com", "P@ssw0rd", new DateTime(1995, 5, 5), app.ClientId);

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _usersRepository.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((UsersEntity?)null);
            _cryptoService.Setup(c => c.HashPassword(request.Password)).Returns("hashed");
            _usersRepository.Setup(r => r.CreateAsync(It.IsAny<UsersEntity>())).ReturnsAsync(7);
            _jwtService.Setup(j => j.GenerateToken(It.IsAny<UsersEntity>(), app.Name, "User")).Returns("sso-token");

            var result = await _sut.RegisterAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal("sso-token", result.Value!.Token);
            _userApplicationsRepository.Verify(r => r.GrantAccessAsync(7, app.Id, "User", 7), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithUnknownOrInactiveApplication_ReturnsFailure()
        {
            var clientId = Guid.NewGuid();
            var request = new SsoRegisterRequest("Jane", "Doe", "jane@test.com", "P@ssw0rd", new DateTime(1995, 5, 5), clientId);

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(clientId)).ReturnsAsync((ApplicationsEntity?)null);

            var result = await _sut.RegisterAsync(request);

            Assert.False(result.IsSuccess);
            _usersRepository.Verify(r => r.CreateAsync(It.IsAny<UsersEntity>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ExistingUserWithCorrectPassword_JoinsNewApplication()
        {
            var user = CreateUser();
            var app = CreateApp();
            var request = new SsoRegisterRequest(user.FirstName, user.LastName, user.Email, "password", user.Birthdate, app.ClientId);

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(user.Id, app.Id)).ReturnsAsync((UserApplicationsEntity?)null);
            _jwtService.Setup(j => j.GenerateToken(user, app.Name, "User")).Returns("joined-token");

            var result = await _sut.RegisterAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal("joined-token", result.Value!.Token);
            _usersRepository.Verify(r => r.CreateAsync(It.IsAny<UsersEntity>()), Times.Never);
            _userApplicationsRepository.Verify(r => r.GrantAccessAsync(user.Id, app.Id, "User", user.Id), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ExistingUserWithWrongPassword_ReturnsFailure()
        {
            var user = CreateUser();
            var app = CreateApp();
            var request = new SsoRegisterRequest(user.FirstName, user.LastName, user.Email, "wrong-password", user.Birthdate, app.ClientId);

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(false);

            var result = await _sut.RegisterAsync(request);

            Assert.False(result.IsSuccess);
            _userApplicationsRepository.Verify(r => r.GrantAccessAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ExistingUserAlreadyActiveInApplication_ReturnsFailure()
        {
            var user = CreateUser();
            var app = CreateApp();
            var grant = new UserApplicationsEntity { Id = 1, UserId = user.Id, ApplicationId = app.Id, Roles = "User", IsActive = true };
            var request = new SsoRegisterRequest(user.FirstName, user.LastName, user.Email, "password", user.Birthdate, app.ClientId);

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(user.Id, app.Id)).ReturnsAsync(grant);

            var result = await _sut.RegisterAsync(request);

            Assert.False(result.IsSuccess);
            _userApplicationsRepository.Verify(r => r.GrantAccessAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ExistingUserWithRevokedGrant_ReactivatesAccessPreservingRole()
        {
            var user = CreateUser();
            var app = CreateApp();
            var grant = new UserApplicationsEntity { Id = 1, UserId = user.Id, ApplicationId = app.Id, Roles = "Admin", IsActive = false };
            var request = new SsoRegisterRequest(user.FirstName, user.LastName, user.Email, "password", user.Birthdate, app.ClientId);

            _applicationsRepository.Setup(r => r.GetByClientIdAsync(app.ClientId)).ReturnsAsync(app);
            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
            _userApplicationsRepository.Setup(r => r.GetGrantAsync(user.Id, app.Id)).ReturnsAsync(grant);
            _jwtService.Setup(j => j.GenerateToken(user, app.Name, "Admin")).Returns("reactivated-token");

            var result = await _sut.RegisterAsync(request);

            Assert.True(result.IsSuccess);
            _userApplicationsRepository.Verify(r => r.GrantAccessAsync(user.Id, app.Id, "Admin", user.Id), Times.Once);
        }
    }
}