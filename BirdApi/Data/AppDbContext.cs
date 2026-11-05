using Microsoft.EntityFrameworkCore;
using BirdApi.Models;

namespace BirdApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<MainTable> MainTables { get; set; }
    public DbSet<ImageBirdTable> ImageBirdTables { get; set; }
    public DbSet<SoundBirdTable> SoundBirdTables { get; set; }
    public DbSet<InfoBirdTable> InfoBirdTables { get; set; }
}