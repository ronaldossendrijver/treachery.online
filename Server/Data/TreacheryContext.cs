using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Treachery.Server;

public partial class TreacheryContext(DbContextOptions<TreacheryContext> options, IConfiguration configuration)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<PersistedGame> PersistedGames { get; set; }
    public DbSet<ArchivedGame> ArchivedGames { get; set; }
    public DbSet<PersistedScheduledGame> ScheduledGames { get; set; }
    public DbSet<ErrorLogEntry> ErrorLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var gameJson = new ValueConverter<string, byte[]>(
            json => CompressedJson.Encode(json),
            data => CompressedJson.Decode(data));

        modelBuilder.Entity<PersistedGame>().Property(g => g.GameState).HasConversion(gameJson);
        modelBuilder.Entity<PersistedGame>().Property(g => g.GameParticipation).HasConversion(gameJson);
        modelBuilder.Entity<ArchivedGame>().Property(g => g.GameState).HasConversion(gameJson);
        modelBuilder.Entity<ArchivedGame>().Property(g => g.GameParticipation).HasConversion(gameJson);
        modelBuilder.Entity<ErrorLogEntry>().HasIndex(entry => entry.OccurredAt);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = configuration.GetConnectionString("TreacheryDatabase");
        optionsBuilder.UseSqlite(connectionString);
    }
}
