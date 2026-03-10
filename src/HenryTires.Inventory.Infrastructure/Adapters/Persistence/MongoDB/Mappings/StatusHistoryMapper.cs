using HenryTires.Inventory.Domain.ValueObjects;
using HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Documents;

namespace HenryTires.Inventory.Infrastructure.Adapters.Persistence.MongoDB.Mappings;

public static class StatusHistoryMapper
{
    public static StatusHistoryEntry<T> ToEntity<T>(StatusHistoryEntryDocument<T> document)
    {
        return new StatusHistoryEntry<T>
        {
            Date = document.Date,
            Status = document.Status,
            User = ToUserLiteEntity(document.User),
            Comment = document.Comment,
        };
    }

    public static StatusHistoryEntryDocument<T> ToDocument<T>(StatusHistoryEntry<T> entity)
    {
        return new StatusHistoryEntryDocument<T>
        {
            Date = entity.Date,
            Status = entity.Status,
            User = ToUserLiteDocument(entity.User),
            Comment = entity.Comment,
        };
    }

    public static UserLite ToUserLiteEntity(UserLiteDocument document)
    {
        return new UserLite
        {
            FirstName = document.FirstName,
            MiddleName = document.MiddleName,
            LastName = document.LastName,
            SecondLastName = document.SecondLastName,
            Username = document.Username,
            Email = document.Email,
        };
    }

    public static UserLiteDocument ToUserLiteDocument(UserLite entity)
    {
        return new UserLiteDocument
        {
            FirstName = entity.FirstName,
            MiddleName = entity.MiddleName,
            LastName = entity.LastName,
            SecondLastName = entity.SecondLastName,
            Username = entity.Username,
            Email = entity.Email,
        };
    }
}
