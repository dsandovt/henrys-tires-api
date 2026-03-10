using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class GroupDocumentMapper
{
    public static Group ToEntity(GroupDocument document)
    {
        return new Group
        {
            Id = document.Id,
            Code = document.Code,
            Name = document.Name,
            Description = document.Description,
            RoleReferences = document.RoleReferences,
            IsActive = document.IsActive,
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy,
        };
    }

    public static GroupDocument ToDocument(Group entity)
    {
        return new GroupDocument
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            RoleReferences = entity.RoleReferences,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy,
        };
    }
}
