using SecureLink.Core.Contracts;
using SecureLink.Core.Entities;

namespace SecureLink.Infrastructure.Helpers;

internal static class Extensions
{
    public static UserResponse ToDto(this User u) =>
        new()
        {
            Id = u.Id,
            Name = u.Name,
            Username = u.Username,
            Email = u.Email,
            LastModifiedAt = u.LastModifiedAt,
            CreatedAt = u.CreatedAt,
        };

    public static FileResponse ToDto(this StoredFile f) =>
        new()
        {
            Id = f.Id,
            UserFilename = f.UserFilename,
            ContentType = f.ContentType,
            Owner = f.Owner,
            LastModifiedAt = f.LastModifiedAt,
            CreatedAt = f.CreatedAt,
        };
}
