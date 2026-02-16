using SecureLink.Core.Entities;

namespace SecureLink.Infrastructure.Contracts;

public record UpdateUserRepoRequest
{
    public required User UpdatedUser { get; init; }
}
