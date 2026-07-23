using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using AuthForge.Tests.TestHelpers;
using Domain.Entities;
using Domain.Interfaces;
using Moq;

namespace AuthForge.Tests.Application
{
    public class AdminServiceTests
    {
        private readonly Mock<IUsersRepository> _usersRepository = new();
        private readonly Mock<ICryptoService> _cryptoService = new();
        private readonly Mock<IJwtService> _jwtService = new();
        private readonly AdminService _sut;

        public AdminServiceTests()
        {
            _sut = new AdminService(
                _usersRepository.Object,
                _cryptoService.Object,
                _jwtService.Object,
                ConfigurationTestHelper.BuildJwtConfiguration());
        }

        [Fact]
        public async Task RegisterAsync_WhenNoPlatformAdminExistsAndEmailNotInUse_CreatesAdminAndReturnsId()
        {
            var request = new RegisterRequest("John", "Doe", "john@test.com", "P@ssw0rd", new DateTime(1990, 1, 1));

            _usersRepository.Setup(r => r.ExistsPlatformAdminAsync()).ReturnsAsync(false);
            _usersRepository.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((UsersEntity?)null);
            _cryptoService.Setup(c => c.HashPassword(request.Password)).Returns("hashed");
            _usersRepository.Setup(r => r.CreateAsync(It.IsAny<UsersEntity>())).ReturnsAsync(42);

            var result = await _sut.RegisterAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
            _usersRepository.Verify(r => r.CreateAsync(It.Is<UsersEntity>(u =>
                u.Email == request.Email && u.PasswordHash == "hashed" && u.IsPlatformAdmin)), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WhenPlatformAdminAlreadyExists_ReturnsFailure()
        {
            var request = new RegisterRequest("John", "Doe", "john@test.com", "P@ssw0rd", new DateTime(1990, 1, 1));

            _usersRepository.Setup(r => r.ExistsPlatformAdminAsync()).ReturnsAsync(true);

            var result = await _sut.RegisterAsync(request);

            Assert.False(result.IsSuccess);
            _usersRepository.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
            _usersRepository.Verify(r => r.CreateAsync(It.IsAny<UsersEntity>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailAlreadyInUse_ReturnsFailure()
        {
            var request = new RegisterRequest("John", "Doe", "john@test.com", "P@ssw0rd", new DateTime(1990, 1, 1));

            _usersRepository.Setup(r => r.ExistsPlatformAdminAsync()).ReturnsAsync(false);
            _usersRepository.Setup(r => r.GetByEmailAsync(request.Email))
                .ReturnsAsync(new UsersEntity { Id = 1, Email = request.Email });

            var result = await _sut.RegisterAsync(request);

            Assert.False(result.IsSuccess);
            _usersRepository.Verify(r => r.CreateAsync(It.IsAny<UsersEntity>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentialsAndPlatformAdmin_ReturnsToken()
        {
            var user = new UsersEntity { Id = 1, Email = "john@test.com", PasswordHash = "hashed", IsPlatformAdmin = true };
            var request = new LoginRequest(user.Email, "P@ssw0rd");

            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
            _jwtService.Setup(j => j.GenerateToken(user, It.IsAny<string?>(), It.IsAny<string?>())).Returns("central-token");

            var result = await _sut.LoginAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal("central-token", result.Value!.Token);
            Assert.Equal(user.Email, result.Value!.Email);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentialsButNotPlatformAdmin_ReturnsFailure()
        {
            // A user who only registered through /users/register for some application (never
            // flagged as the platform admin) must not be able to obtain a central token here,
            // even with correct credentials for their own account.
            var user = new UsersEntity { Id = 1, Email = "enduser@test.com", PasswordHash = "hashed", IsPlatformAdmin = false };
            var request = new LoginRequest(user.Email, "P@ssw0rd");

            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);

            var result = await _sut.LoginAsync(request);

            Assert.False(result.IsSuccess);
            _jwtService.Verify(j => j.GenerateToken(It.IsAny<UsersEntity>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
        {
            var user = new UsersEntity { Id = 1, Email = "john@test.com", PasswordHash = "hashed", IsPlatformAdmin = true };
            var request = new LoginRequest(user.Email, "wrong-password");

            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _cryptoService.Setup(c => c.VerifyPassword(request.Password, user.PasswordHash)).Returns(false);

            var result = await _sut.LoginAsync(request);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task LoginAsync_WithUnknownEmail_ReturnsFailure()
        {
            var request = new LoginRequest("ghost@test.com", "whatever");

            _usersRepository.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((UsersEntity?)null);

            var result = await _sut.LoginAsync(request);

            Assert.False(result.IsSuccess);
        }
    }
}