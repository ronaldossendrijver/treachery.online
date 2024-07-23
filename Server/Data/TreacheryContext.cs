using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
//using Treachery.Server.Migrations;

namespace Treachery.Shared;

public partial class TreacheryContext(DbContextOptions<TreacheryContext> options, IConfiguration configuration)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<PlayedGame> Games { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = configuration.GetConnectionString("TreacheryDatabase");
        optionsBuilder.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
