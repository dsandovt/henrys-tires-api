using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class UserDocumentMapper
{
    public static User ToEntity(UserDocument document)
    {
        return new User
        {
            Id = document.Id,
            Username = document.Username,
            PasswordHash = document.PasswordHash,
            Role = document.Role,
            BranchId = document.BranchId,
            IsActive = document.IsActive,
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy
        };
    }

    public static UserDocument ToDocument(User entity)
    {
        return new UserDocument
        {
            Id = entity.Id,
            Username = entity.Username,
            PasswordHash = entity.PasswordHash,
            Role = entity.Role,
            BranchId = entity.BranchId,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy
        };
    }
}
