using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CourseEvaluation.Core.DTOs;
using CourseEvaluation.Core.Entities;
using CourseEvaluation.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CourseEvaluation.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("用户名或密码错误");
        }

        var token = GenerateJwtToken(user.Id, user.Username, user.Role.ToString());

        return new LoginResponse(
            Token: token,
            Username: user.Username,
            Nickname: user.Nickname,
            Role: user.Role.ToString());
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
        {
            throw new InvalidOperationException("用户名已存在");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = HashPassword(request.Password),
            Nickname = request.Nickname,
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new UserDto(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            Nickname: user.Nickname,
            Role: user.Role.ToString(),
            CreatedAt: user.CreatedAt);
    }

    public string GenerateJwtToken(Guid userId, string username, string role)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
