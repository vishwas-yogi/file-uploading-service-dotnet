namespace SecureLink.Infrastructure.Contracts;

public record CreateUserRepoRequest(
    Guid Id,
    string Username,
    string Name,
    string? Email,
    string PasswordHash
);
