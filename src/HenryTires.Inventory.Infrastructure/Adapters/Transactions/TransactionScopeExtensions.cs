using HenryTires.Inventory.Application.Ports.Outbound;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Adapters.Transactions;

/// <summary>
/// Extension methods for converting ITransactionScope to MongoDB-specific types.
/// Internal to Infrastructure layer only.
/// </summary>
internal static class TransactionScopeExtensions
{
    /// <summary>
    /// Extracts the MongoDB IClientSessionHandle from an ITransactionScope.
    /// Throws if the scope is not a MongoTransactionScope.
    /// </summary>
    internal static IClientSessionHandle? ToMongoSession(this ITransactionScope? transactionScope)
    {
        if (transactionScope == null)
            return null;

        if (transactionScope is MongoTransactionScope mongoScope)
            return mongoScope.Session;

        throw new InvalidOperationException(
            $"Transaction scope must be a {nameof(MongoTransactionScope)} for MongoDB repositories.");
    }
}
