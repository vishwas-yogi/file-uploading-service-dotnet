namespace SecureLink.Core.Contracts;

public record CreateUserRequest(string Name, string Username, string? Email, string PasswordHash);
