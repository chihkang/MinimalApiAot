namespace MinimalApiAot.Services;

public class UserService(ApplicationDbContext context) : IUserService
{
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await context.Users
            .AsNoTracking() // 提高讀取效能
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return null;

        return await context.Users.FindAsync(objectId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User> CreateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        user.CreatedAt = DateTime.UtcNow;

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> UpdateUserAsync(string id, User user)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return false;

        var existingUser = await context.Users.FindAsync(objectId);
        if (existingUser == null)
            return false;

        var originalCreatedAt = existingUser.CreatedAt;
        context.Entry(existingUser).CurrentValues.SetValues(user);
        existingUser.CreatedAt = originalCreatedAt;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
            return false;

        var user = await context.Users.FindAsync(objectId);
        if (user == null)
            return false;

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return true;
    }
}