using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class RoleDocumentMapper
{
    public static Role ToEntity(RoleDocument document)
    {
        return new Role
        {
            Id = document.Id,
            Code = document.Code,
            Name = document.Name,
            Description = document.Description,
            IsActive = document.IsActive,
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy,
        };
    }

    public static RoleDocument ToDocument(Role entity)
    {
        return new RoleDocument
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy,
        };
    }
}
