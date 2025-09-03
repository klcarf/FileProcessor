using Microsoft.EntityFrameworkCore;
using Models;

namespace Metadata
{
    public class FileProcessDbContext : DbContext
    {
        public FileProcessDbContext(DbContextOptions<FileProcessDbContext> options) : base(options) { }

        public DbSet<Models.File> Files { get; set; }
        public DbSet<Chunk> Chunks { get; set; }
        public DbSet<Folder> Folders { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.File>(e =>
            {
                e.HasKey(f => f.Id);
                e.Property(f => f.FileName).IsRequired();
                e.Property(f => f.HashSha256).IsRequired();
                e.HasIndex(f => f.UpdateDate);
            });


            modelBuilder.Entity<Chunk>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.HashSha256).IsRequired();
                e.Property(c => c.Provider).IsRequired();
                e.Property(c => c.ProviderKey).IsRequired();
                e.HasIndex(c => new { c.FileId, c.Index }).IsUnique();
                e.HasOne<Models.File>()
                .WithMany()
                .HasForeignKey(c => c.FileId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Folder>(e =>
            {
                e.HasKey(f => f.Id);
                e.Property(f => f.RootPath).IsRequired();
                e.Property(f => f.RootName).IsRequired();
                e.HasIndex(f => f.CreateDate);
            });


        }
    }
}
