using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess
{
    public class DatabaseStorageDbContext : DbContext
    {
        public DatabaseStorageDbContext(DbContextOptions<DatabaseStorageDbContext> options) : base(options) { }

        public DbSet<Storage> Storages { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Storage>(e =>
            {
                e.HasKey(f => f.Id);
                e.Property(f => f.FileId).IsRequired();
                e.Property(f => f.ChunkData).IsRequired();
                e.Property(f => f.ChunkOrder).IsRequired();
                e.HasIndex(f => f.CreateDate);
            });

        }
    }
}
