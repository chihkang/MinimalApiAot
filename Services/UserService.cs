using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MinimalApiAot.Services;

public class UserService(MongoDbContext db, ILogger<UserService> logger, IPortfolioService portfolioService)
    : IUserService
{
    private readonly IMongoCollection<User> _users = db.Users;
    private readonly IMongoCollection<Portfolio> _portfolios = db.Portfolios;
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _users.Find(FilterDefinition<User>.Empty).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(ObjectId objectId)
    {
        return await _users.Find(u => u.Id == objectId).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        var escaped = Regex.Escape(email);
        var regex = new BsonRegularExpression($"^{escaped}$", "i");
        var filter = Builders<User>.Filter.Regex(u => u.Email, regex);

        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            // 設置創建時間
            user.CreatedAt = DateTime.UtcNow;
            if (user.Id == ObjectId.Empty)
            {
                user.Id = ObjectId.GenerateNewId();
            }

            // 創建用戶
            await _users.InsertOneAsync(user);

            // 自動創建 Portfolio
            var portfolio = await portfolioService.CreateAsync(user.Id);

            // 更新用戶的 PortfolioId
            user.PortfolioId = portfolio.Id;
            var update = Builders<User>.Update.Set(u => u.PortfolioId, portfolio.Id);
            await _users.UpdateOneAsync(u => u.Id == user.Id, update);

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "創建用戶時發生錯誤");

            // 如果創建 Portfolio 後更新用戶失敗，嘗試清理已創建的資料
            try
            {
                if (user.Id != default)
                {
                    await _users.DeleteOneAsync(u => u.Id == user.Id);
                    await _portfolios.DeleteOneAsync(p => p.UserId == user.Id);
                }
            }
            catch (Exception cleanupEx)
            {
                logger.LogError(cleanupEx, "清理失敗的用戶創建數據時發生錯誤");
            }

            throw;
        }
    }


    public async Task<bool> UpdateUserAsync(ObjectId objectId, User user)
    {
        var existingUser = await _users.Find(u => u.Id == objectId).FirstOrDefaultAsync();
        if (existingUser == null)
            return false;

        user.Id = objectId;
        user.CreatedAt = existingUser.CreatedAt;

        var result = await _users.ReplaceOneAsync(u => u.Id == objectId, user);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteUserAsync(ObjectId objectId)
    {
        try
        {
            var user = await _users.Find(u => u.Id == objectId).FirstOrDefaultAsync();
            if (user == null)
                return false;

            // 先刪除關聯的投資組合
            await _portfolios.DeleteOneAsync(p => p.UserId == objectId);

            // 再刪除使用者
            await _users.DeleteOneAsync(u => u.Id == objectId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刪除使用者及其投資組合時發生錯誤。使用者ID: {UserId}", objectId);
            throw;
        }
    }
}