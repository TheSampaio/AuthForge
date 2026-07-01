using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Moq;

namespace AuthForge.Tests.Application
{
    public class UsersServiceTests
    {
        private readonly Mock<IUsersRepository> _usersRepository = new();
        private readonly UsersService _sut;

        public UsersServiceTests()
        {
            _sut = new UsersService(_usersRepository.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedUsers()
        {
            var users = new[]
            {
                new UsersEntity { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", Birthdate = new DateTime(1990, 1, 1) },
                new UsersEntity { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", Birthdate = new DateTime(1992, 2, 2) }
            };

            _usersRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await _sut.GetAllAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value!.Count());
            Assert.Contains(result.Value!, u => u.Email == "john@test.com");
        }

        [Fact]
        public async Task GetByEmailAsync_Found_ReturnsUser()
        {
            var user = new UsersEntity { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@test.com", Birthdate = new DateTime(1990, 1, 1) };

            _usersRepository.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var result = await _sut.GetByEmailAsync(user.Email);

            Assert.True(result.IsSuccess);
            Assert.Equal(user.Email, result.Value!.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_NotFound_ReturnsFailure()
        {
            _usersRepository.Setup(r => r.GetByEmailAsync("ghost@test.com")).ReturnsAsync((UsersEntity?)null);

            var result = await _sut.GetByEmailAsync("ghost@test.com");

            Assert.False(result.IsSuccess);
        }
    }
}