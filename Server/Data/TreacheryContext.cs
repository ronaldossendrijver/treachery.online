using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Treachery.Server;

public partial class TreacheryContext(DbContextOptions<TreacheryContext> options, IConfiguration configuration)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<PersistedGame> PersistedGames { get; set; }
    public DbSet<ArchivedGame> ArchivedGames { get; set; }
    public DbSet<PersistedScheduledGame> ScheduledGames { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = configuration.GetConnectionString("TreacheryDatabase");
        optionsBuilder.UseSqlite(connectionString);
    }
}
