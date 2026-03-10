using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class BranchDocumentMapper
{
    public static Branch ToEntity(BranchDocument document)
    {
        return new Branch
        {
            Id = document.Id,
            Code = document.Code,
            Name = document.Name,
            Address = document.Address,
            Phone = document.Phone,
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy,
        };
    }

    public static BranchDocument ToDocument(Branch entity)
    {
        return new BranchDocument
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Address = entity.Address,
            Phone = entity.Phone,
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy,
        };
    }
}
