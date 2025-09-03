using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Storage
{
    public class DatabaseStorageProvider : IStorageProvider
    {
        public string Name { get; set; }
        private readonly DatabaseStorageDbContext _context;

        public DatabaseStorageProvider(string name, DatabaseStorageDbContext context)
        {
            Name = name;
            _context = context;
        }

        public async Task AddChunkAsync(string fileId, int chunkOrder, byte[] data)
        {
            var existingChunk = await _context.Storages
                .FirstOrDefaultAsync(c => c.FileId == fileId && c.ChunkOrder == chunkOrder);

            if (existingChunk != null)
            {
                existingChunk.ChunkData = data;
            }
            else
            {
                var newChunk = new Models.Storage
                {
                    Id = new Guid(),
                    FileId = fileId,
                    ChunkOrder = chunkOrder,
                    ChunkData = data,
                    CreateDate = DateTime.UtcNow

                };
                _context.Storages.Add(newChunk);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<byte[]> GetChunkAsync(string fileId, int chunkOrder)
        {
            var chunk = await _context.Storages
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.FileId == fileId && c.ChunkOrder == chunkOrder);

            return chunk?.ChunkData;
        }

        public async Task DeleteFileChunksAsync(string fileId)
        {
            await _context.Storages
                .Where(c => c.FileId == fileId)
                .ExecuteDeleteAsync();   
        }
    }
}
