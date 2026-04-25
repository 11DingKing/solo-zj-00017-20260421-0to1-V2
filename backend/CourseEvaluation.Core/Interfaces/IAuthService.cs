using CourseEvaluation.Core.DTOs;

namespace CourseEvaluation.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    string GenerateJwtToken(Guid userId, string username, string role);
    bool VerifyPassword(string password, string passwordHash);
    string HashPassword(string password);
}
