namespace MinimalApiAot.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }

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
            
            entity.OwnsOne(e => e.Settings, settings =>
            {
                settings.WithOwner();
            });
        });
    }
}