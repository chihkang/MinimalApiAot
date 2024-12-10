namespace MinimalApiAot.Services;

public class UserService(ApplicationDbContext context, ILogger<UserService> logger, IPortfolioService portfolioService)
    : IUserService
{
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await context.Users
            .AsNoTracking() // 提高讀取效能
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(ObjectId objectId)
    {
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
        try
        {
            // 設置創建時間
            user.CreatedAt = DateTime.UtcNow;

            // 創建用戶
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // 自動創建 Portfolio
            var portfolio = await portfolioService.CreateAsync(user.Id);

            // 更新用戶的 PortfolioId
            user.PortfolioId = portfolio.Id;
            context.Users.Update(user);
            await context.SaveChangesAsync();

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
                    var existingUser = await context.Users.FindAsync(user.Id);
                    if (existingUser != null)
                    {
                        context.Users.Remove(existingUser);
                        await context.SaveChangesAsync();
                    }
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
        var existingUser = await context.Users.FindAsync(objectId);
        if (existingUser == null)
            return false;

        var originalCreatedAt = existingUser.CreatedAt;
        context.Entry(existingUser).CurrentValues.SetValues(user);
        existingUser.CreatedAt = originalCreatedAt;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(ObjectId objectId)
    {
        try
        {
            var user = await context.Users.FindAsync(objectId);
            if (user == null)
                return false;

            // 先刪除關聯的投資組合
            var portfolio = await context.Portfolios.FirstOrDefaultAsync(p => p.UserId == objectId);
            if (portfolio != null)
            {
                context.Portfolios.Remove(portfolio);
                await context.SaveChangesAsync();
            }

            // 再刪除使用者
            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刪除使用者及其投資組合時發生錯誤。使用者ID: {UserId}", objectId);
            throw;
        }
    }
}