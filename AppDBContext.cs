using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<GameResult> GameResults { get; set; }

    public async Task CreateGameResultAsync(GameResult gameResult)
    {
        GameResults.Add(gameResult);
        await SaveChangesAsync();
    }
}
