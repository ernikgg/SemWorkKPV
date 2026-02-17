using Microsoft.EntityFrameworkCore;
using SemWorkKPV.Models;

namespace SemWorkKPV.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AppUser> Users => Set<AppUser>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Markdown).IsRequired();
            e.Property(x => x.Html).IsRequired();
        });
    }
}