namespace SecureLink.Core.Contracts;

public record UpdateUserRequest(Guid Id, string? Username, string? Name, string? Email);
