using Microsoft.EntityFrameworkCore;
using PSXMaster.Database.Tables;

namespace PSXMaster.Database;
public class PSXMasterDbContext : DbContext
{
    public PSXMasterDbContext()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string filename = $"{AppContext.BaseDirectory}Games.db";
        if (!string.IsNullOrEmpty(Settings.DBPath))
        {
            filename = $"{Settings.DBPath}";
        }
        optionsBuilder.UseSqlite($"Data Source={filename}");
    }

    public DbSet<QueueGames> QueueGames { get; set; }
}
