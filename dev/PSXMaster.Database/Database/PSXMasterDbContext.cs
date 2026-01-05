using Microsoft.EntityFrameworkCore;
using PSXMaster.Database.CompiledModels;
using PSXMaster.Database.Tables;

namespace PSXMaster.Database;
public partial class PSXMasterDbContext : DbContext
{
    public static string DatabasePath { get; set; } = string.Empty;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {        
        optionsBuilder.UseModel(PSXMasterDbContextModel.Instance).UseSqlite($"Data Source={DatabasePath}");
    }

    public DbSet<QueueGames> QueueGames { get; set; }
}
