namespace SecureLink.Infrastructure.Contracts;

public record FileGetRepoRequest
{
    public required Guid Id { get; init; }
    public required Guid Owner { get; set; }
}
