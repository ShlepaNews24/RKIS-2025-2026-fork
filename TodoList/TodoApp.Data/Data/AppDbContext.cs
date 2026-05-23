using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Profile> Profiles => Set<Profile>();
        public DbSet<TodoItem> TodoItems => Set<TodoItem>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=todos.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profile>()
                .HasMany(p => p.TodoItems)
                .WithOne(t => t.Profile)
                .HasForeignKey(t => t.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
                .HasIndex(p => p.Login)
                .IsUnique();

            modelBuilder.Entity<TodoItem>()
                .Property(t => t.LastUpdate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}