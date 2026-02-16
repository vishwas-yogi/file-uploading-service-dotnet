namespace SecureLink.Core.Contracts;

public record RegisterRequest(string Username, string Password, string Name, string? Email);
