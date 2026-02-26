namespace SecureLink.Core.Contracts;

public enum FileStatus
{
    Available,
    Pending,
    CleanupRequired,
    Deleted, // For soft delete
}
