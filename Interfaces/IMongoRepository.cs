namespace MinimalApiAot.Interfaces;

public interface IMongoRepository
{
    Task<IEnumerable<T>> GetAllAsync<T>() where T : class;
    Task<T?> GetByIdAsync<T>(string id) where T : class;
    Task CreateAsync<T>(T entity) where T : class;
    Task UpdateAsync<T>(string id, T entity) where T : class;
    Task DeleteAsync<T>(string id) where T : class;
}