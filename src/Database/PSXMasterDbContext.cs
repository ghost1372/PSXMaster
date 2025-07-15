using Microsoft.EntityFrameworkCore;
using PSXMaster.Common;
using PSXMaster.Database.Tables;

namespace PSXMaster.Database;
public partial class PSXMasterDbContext : DbContext
{
    public PSXMasterDbContext()
    {
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string filename = Constants.DatabaseFilePath;
        if (!string.IsNullOrEmpty(Settings.DBPath))
        {
            filename = $"{Settings.DBPath}";
        }
        optionsBuilder.UseSqlite($"Data Source={filename}");
    }

    public DbSet<QueueGames> QueueGames { get; set; }
}
