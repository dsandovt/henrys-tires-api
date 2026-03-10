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
            FirstName = document.FirstName,
            MiddleName = document.MiddleName,
            LastName = document.LastName,
            SecondLastName = document.SecondLastName,
            Email = document.Email,
            GroupReferences = document.GroupReferences,
            BranchReferences = document.BranchReferences,
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
            FirstName = entity.FirstName,
            MiddleName = entity.MiddleName,
            LastName = entity.LastName,
            SecondLastName = entity.SecondLastName,
            Email = entity.Email,
            GroupReferences = entity.GroupReferences,
            BranchReferences = entity.BranchReferences,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy
        };
    }
}
