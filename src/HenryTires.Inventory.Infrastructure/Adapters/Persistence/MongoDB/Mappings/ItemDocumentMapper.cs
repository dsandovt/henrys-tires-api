using HenryTires.Inventory.Domain.Entities;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class ItemDocumentMapper
{
    public static Item ToEntity(ItemDocument document)
    {
        return new Item
        {
            Id = document.Id,
            ItemCode = document.ItemCode,
            Description = document.Description,
            Classification = document.Classification,
            Category = document.Category,
            Brand = document.Brand,
            Size = document.Size,
            Notes = document.Notes,
            IsActive = document.IsActive,
            IsDeleted = document.IsDeleted,
            DeletedAtUtc = document.DeletedAtUtc,
            DeletedBy = document.DeletedBy,
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy,
            ModifiedAtUtc = document.ModifiedAtUtc,
            ModifiedBy = document.ModifiedBy
        };
    }

    public static ItemDocument ToDocument(Item entity)
    {
        return new ItemDocument
        {
            Id = entity.Id,
            ItemCode = entity.ItemCode,
            Description = entity.Description,
            Classification = entity.Classification,
            Category = entity.Category,
            Brand = entity.Brand,
            Size = entity.Size,
            Notes = entity.Notes,
            IsActive = entity.IsActive,
            IsDeleted = entity.IsDeleted,
            DeletedAtUtc = entity.DeletedAtUtc,
            DeletedBy = entity.DeletedBy,
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            ModifiedBy = entity.ModifiedBy
        };
    }
}
