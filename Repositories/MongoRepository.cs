namespace MinimalApiAot.Repositories;

public class MongoRepository(IMongoClient client, IOptions<MongoSettings> settings) : IMongoRepository
{
    private readonly IMongoDatabase _database = client.GetDatabase(settings.Value.DatabaseName);

    public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
        => await _database.GetCollection<T>(typeof(T).Name)
            .Find(_ => true)
            .ToListAsync();

    public async Task<T?> GetByIdAsync<T>(string id) where T : class
        => await _database.GetCollection<T>(typeof(T).Name)
            .Find(Builders<T>.Filter.Eq("_id", id))
            .FirstOrDefaultAsync();

    public async Task CreateAsync<T>(T entity) where T : class
        => await _database.GetCollection<T>(typeof(T).Name)
            .InsertOneAsync(entity);

    public async Task UpdateAsync<T>(string id, T entity) where T : class
        => await _database.GetCollection<T>(typeof(T).Name)
            .ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity);

    public async Task DeleteAsync<T>(string id) where T : class
        => await _database.GetCollection<T>(typeof(T).Name)
            .DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
}