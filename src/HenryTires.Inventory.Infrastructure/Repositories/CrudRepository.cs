using MongoDB.Bson;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Repositories;

public abstract class CrudRepository<TDocument>
    where TDocument : class
{
    public readonly IMongoCollection<TDocument> _collection;
    private readonly string _documentId;

    public CrudRepository(
        IMongoClient mongoClient,
        string databaseName,
        string? collectionName = null,
        string documentId = "_id"
    )
    {
        IMongoDatabase db = mongoClient.GetDatabase(databaseName);
        _collection = db.GetCollection<TDocument>(
            collectionName is not null ? collectionName : typeof(TDocument).Name
        );
        _documentId = documentId;
    }

    public async Task<TDocument> UpsertAsync(
        string? id,
        TDocument document,
        IClientSessionHandle? session = null
    )
    {
        if (string.IsNullOrEmpty(id))
        {
            if (session != null)
            {
                await _collection.InsertOneAsync(session, document);
            }
            else
            {
                await _collection.InsertOneAsync(document);
            }
            return document;
        }

        FilterDefinition<TDocument> filter =
            _documentId == "_id"
                ? Builders<TDocument>.Filter.Eq(_documentId, new ObjectId(id))
                : Builders<TDocument>.Filter.Eq(_documentId, id);

        ReplaceOptions options = new ReplaceOptions { IsUpsert = true };

        if (session != null)
        {
            await _collection.ReplaceOneAsync(session, filter, document, options);
        }
        else
        {
            await _collection.ReplaceOneAsync(filter, document, options);
        }

        return document;
    }

    // Get document by ID (no session needed as it's a read operation)
    public async Task<TDocument?> GetByIdAsync(string id)
    {
        FilterDefinition<TDocument> filter =
            _documentId == "_id"
                ? Builders<TDocument>.Filter.Eq(_documentId, new ObjectId(id))
                : Builders<TDocument>.Filter.Eq(_documentId, id);

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    // Get all documents in the collection (no session needed as it's a read operation)
    public async Task<IEnumerable<TDocument>> GetAllAsync()
    {
        FilterDefinition<TDocument> filter = FilterDefinition<TDocument>.Empty;
        return await _collection.Find(filter).ToListAsync();
    }

    // Delete document by ID with optional session
    public virtual async Task<DeleteResult> DeleteByIdAsync(
        string id,
        IClientSessionHandle? session = null
    )
    {
        FilterDefinition<TDocument> filter =
            _documentId == "_id"
                ? Builders<TDocument>.Filter.Eq(_documentId, new ObjectId(id))
                : Builders<TDocument>.Filter.Eq(_documentId, id);

        if (session != null)
        {
            return await _collection.DeleteOneAsync(session, filter);
        }
        else
        {
            return await _collection.DeleteOneAsync(filter);
        }
    }

    // Find first document in the collection (no session needed as it's a read operation)
    public async Task<TDocument?> GetFirstDocumentAsync()
    {
        FilterDefinition<TDocument> filter = FilterDefinition<TDocument>.Empty;
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TDocument>> GetByIdsAsync(IEnumerable<string> ids)
    {
        FilterDefinition<TDocument> filter =
            _documentId == "_id"
                ? Builders<TDocument>.Filter.In(_documentId, ids.Select(ObjectId.Parse))
                : Builders<TDocument>.Filter.In(_documentId, ids);

        return await _collection.Find(filter).ToListAsync();
    }
}
