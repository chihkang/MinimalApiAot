using MinimalApiAot.Models.Entities;

namespace MinimalApiAot.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(ObjectId id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<bool> UpdateUserAsync(ObjectId id, User user);
    Task<bool> DeleteUserAsync(ObjectId id);
}