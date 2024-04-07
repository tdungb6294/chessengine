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
        if (GameResults.Any(gr => gr.PlayerName1 == gameResult.PlayerName1 && gr.PlayerName2 == gameResult.PlayerName2 && gr.GameStatus == gameResult.GameStatus))
        {
            return;
        }
        GameResults.Add(gameResult);
        await SaveChangesAsync();
    }
}
