namespace CourseEvaluation.Core.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, string Username, string Nickname, string Role);

public record RegisterRequest(string Username, string Email, string Password, string Nickname);

public record UserDto(Guid Id, string Username, string Email, string Nickname, string Role, DateTime CreatedAt);
