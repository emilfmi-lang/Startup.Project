

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Services.Concretes.Account;
using Startup_Project.Domain.Entities.Identity;
using Xunit;

namespace StartupProject.Tests.Unit;

public class AccountServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IMailService> _mailServiceMock;
    private readonly AccountService _accountService;

    public AccountServiceTests()
    {
        _userManagerMock = GetUserManagerMock();
        _mapperMock = new Mock<IMapper>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _mailServiceMock = new Mock<IMailService>();

        _accountService = new AccountService(
            _userManagerMock.Object,
            _mapperMock.Object,
            _jwtTokenServiceMock.Object,
            _mailServiceMock.Object
        );
    }

    private Mock<UserManager<AppUser>> GetUserManagerMock()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "test@test.com", Password = "Test123!" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync((AppUser)null);

        // Act
        var result = await _accountService.LoginAsync(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("İstifadəçi tapılmadı.", result.Errors);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsFailure()
        {
            // Arrange
            var loginDto = new UserLoginDto { Email = "test@test.com", Password = "WrongPassword" };
            var user = new AppUser { Email = loginDto.Email, EmailConfirmed = true };

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _accountService.LoginAsync(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Email və ya şifrə yanlışdır.", result.Errors);
        }

        [Fact]
        public async Task LoginAsync_EmailNotConfirmed_ReturnsFailure()
        {
            // Arrange
            var loginDto = new UserLoginDto { Email = "test@test.com", Password = "Test123!" };
            var user = new AppUser { Email = loginDto.Email, EmailConfirmed = false };

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);

            // Act
            var result = await _accountService.LoginAsync(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Zəhmət olmasa əvvəlcə emailinizi təsdiqləyin.", result.Errors);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "test@test.com", Password = "Test123!" };
        var user = new AppUser { Email = loginDto.Email, EmailConfirmed = true };
        var roles = new List<string> { "User" };
        var token = "generated-jwt-token";

        _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);
        _jwtTokenServiceMock.Setup(x => x.GenerateTokenAsync(user, roles))
            .Returns(token);

        // Act
        var result = await _accountService.LoginAsync(loginDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(token, result.Data);
    }
}
