namespace MinimalApiAot.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<Stock> Stocks { get; init; }

    public DbSet<Portfolio> Portfolios { get; init; }

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

            entity.OwnsMany(e => e.Stocks);
        });
    }
}