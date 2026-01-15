namespace MinimalApiAot.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<Stock> Stocks { get; init; }

    public DbSet<Portfolio> Portfolios { get; init; }
    public DbSet<PortfolioDailyValue> PortfolioDailyValues { get; init; }
    public DbSet<PositionEvent> PositionEvents { get; init; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToCollection("users");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Username)
                .IsRequired();

            entity.Property(e => e.Email)
                .IsRequired();

            entity.Property(e => e.PortfolioId)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.OwnsOne(e => e.Settings, settings => { settings.WithOwner(); });
            // 修改關聯配置
            entity.HasOne(u => u.Portfolio) // 指定導航屬性
                .WithOne(p => p.User) // 指定反向導航屬性
                .HasForeignKey<Portfolio>(p => p.UserId);
        });
        modelBuilder.Entity<Stock>(entity =>
        {
            entity.ToCollection("stocks");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired();

            entity.Property(e => e.Alias)
                .IsRequired();

            entity.Property(e => e.Price)
                .IsRequired();

            entity.Property(e => e.Currency)
                .IsRequired();

            entity.Property(e => e.LastUpdated)
                .IsRequired();
        });
        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.ToCollection("portfolio");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.LastUpdated)
                .IsRequired();

            // Version field for optimistic concurrency
            entity.Property(e => e.Version)
                .IsConcurrencyToken();

            entity.OwnsMany(e => e.Stocks);
        });
        modelBuilder.Entity<PortfolioDailyValue>(entity =>
        {
            entity.ToCollection("portfolio_daily_values");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PortfolioId)
                .IsRequired();
            entity.Property(e => e.TotalValueTwd)
                .IsRequired();
        });

        modelBuilder.Entity<PositionEvent>(entity =>
        {
            entity.ToCollection("positionEvents");
            entity.HasKey(e => e.Id);

            // Required properties
            entity.Property(e => e.OperationId)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.Property(e => e.StockId)
                .IsRequired();

            entity.Property(e => e.Type)
                .IsRequired();

            entity.Property(e => e.TradeAt)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.Currency)
                .IsRequired();

            entity.Property(e => e.Source)
                .IsRequired();

            entity.Property(e => e.AppVersion)
                .IsRequired();

            // Unique index on operationId
            entity.HasIndex(e => e.OperationId)
                .IsUnique();

            // Compound index on userId + tradeAt (descending) for user queries
            entity.HasIndex(e => new { e.UserId, e.TradeAt });

            // Compound index on stockId + tradeAt (descending) for stock queries
            entity.HasIndex(e => new { e.StockId, e.TradeAt });
        });
    }
}