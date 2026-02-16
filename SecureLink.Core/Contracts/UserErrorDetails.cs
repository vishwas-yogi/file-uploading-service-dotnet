namespace SecureLink.Core.Contracts;

public class UserErrorDetails
{
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; } // For general purpose error.
}
