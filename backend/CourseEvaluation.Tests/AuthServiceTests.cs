using CourseEvaluation.Core.Entities;
using CourseEvaluation.Core.Interfaces;
using CourseEvaluation.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CourseEvaluation.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Secret", "YourSuperSecretJwtKeyHereMakeItLongEnough32Chars"},
            {"Jwt:Issuer", "CourseEvaluationApi"},
            {"Jwt:Audience", "CourseEvaluationClient"},
            {"Jwt:ExpiryInMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _authService = new AuthService(_userRepositoryMock.Object, _configuration);
    }

    #region Password Hashing Tests

    [Fact]
    public void HashPassword_ValidPassword_GeneratesValidHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _authService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        BCrypt.Net.BCrypt.Verify(password, hash).Should().BeTrue();
    }

    [Fact]
    public void HashPassword_SamePasswords_GenerateDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _authService.HashPassword(password);
        var hash2 = _authService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region Password Verification Tests

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region JWT Token Tests

    [Fact]
    public void GenerateJwtToken_ValidCredentials_GeneratesValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "Student";

        // Act
        var token = _authService.GenerateJwtToken(userId, username, role);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3);
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task RegisterAsync_ExistingUsername_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CourseEvaluation.Core.DTOs.RegisterRequest(
            Username: "existinguser",
            Email: "test@example.com",
            Password: "TestPassword123!",
            Nickname: "Test User");

        _userRepositoryMock
            .Setup(r => r.ExistsByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("用户名已存在");
    }

    [Fact]
    public async Task RegisterAsync_NewUsername_CreatesUser()
    {
        // Arrange
        var request = new CourseEvaluation.Core.DTOs.RegisterRequest(
            Username: "newuser",
            Email: "new@example.com",
            Password: "TestPassword123!",
            Nickname: "New User");

        _userRepositoryMock
            .Setup(r => r.ExistsByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u) => u);

        _userRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Username.Should().Be(request.Username);
        result.Email.Should().Be(request.Email);
        result.Nickname.Should().Be(request.Nickname);
        result.Role.Should().Be("Student");

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.Is<User>(u => 
                u.Username == request.Username && 
                u.Email == request.Email &&
                u.Role == UserRole.Student),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task LoginAsync_NonExistentUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new CourseEvaluation.Core.DTOs.LoginRequest(
            Username: "nonexistent",
            Password: "TestPassword123!");

        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("用户名或密码错误");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new CourseEvaluation.Core.DTOs.LoginRequest(
            Username: "testuser",
            Password: "WrongPassword!");

        var correctHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword!");
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Nickname = "Test User",
            PasswordHash = correctHash,
            Role = UserRole.Student
        };

        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("用户名或密码错误");
    }

    [Fact]
    public async Task LoginAsync_CorrectCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Nickname = "Test User",
            PasswordHash = hash,
            Role = UserRole.Student
        };

        var request = new CourseEvaluation.Core.DTOs.LoginRequest(
            Username: user.Username,
            Password: password);

        _userRepositoryMock
            .Setup(r => r.GetByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Username.Should().Be(user.Username);
        result.Nickname.Should().Be(user.Nickname);
        result.Role.Should().Be(UserRole.Student.ToString());
        result.Token.Should().NotBeNullOrEmpty();
    }

    #endregion
}
