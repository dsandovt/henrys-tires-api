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
            Name = document.Name
        };
    }

    public static BranchDocument ToDocument(Branch entity)
    {
        return new BranchDocument
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name
        };
    }
}
