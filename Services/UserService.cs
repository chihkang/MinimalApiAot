namespace MinimalApiAot.Services;

public class UserService(IMongoDatabase database) : IUserService
{
    private readonly IMongoCollection<User> _users = database.GetCollection<User>("users");

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _users.Find(_ => true).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User> CreateUserAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;
        await _users.InsertOneAsync(user);
        return user;
    }

    public async Task<bool> UpdateUserAsync(string id, User updatedUser)
    {
        var result = await _users.ReplaceOneAsync(
            u => u.Id == id,
            updatedUser,
            new ReplaceOptions { IsUpsert = false });

        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }
}