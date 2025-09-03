using Metadata;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Storage
{
    public class DatabaseStorageProvider : IStorageProvider
    {
        public string Name { get; set; }
        private readonly FileProcessDbContext _context;

        public DatabaseStorageProvider(string name, FileProcessDbContext context)
        {
            Name = name;
            _context = context;
        }

        public async Task AddChunkAsync(string fileId, int chunkOrder, byte[] data)
        {
            var existingChunk = await _context.DBStorages
                .FirstOrDefaultAsync(c => c.FileId == fileId && c.ChunkOrder == chunkOrder);

            if (existingChunk != null)
            {
                existingChunk.ChunkData = data;
            }
            else
            {
                var newChunk = new DBStorage
                {
                    Id = new Guid(),
                    FileId = fileId,
                    ChunkOrder = chunkOrder,
                    ChunkData = data,
                    CreateDate = DateTime.UtcNow

                };
                _context.DBStorages.Add(newChunk);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<byte[]> GetChunkAsync(string fileId, int chunkOrder)
        {
            var chunk = await _context.DBStorages
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.FileId == fileId && c.ChunkOrder == chunkOrder);

            return chunk?.ChunkData;
        }

        public async Task DeleteFileChunksAsync(string fileId)
        {
            await _context.DBStorages
                .Where(c => c.FileId == fileId)
                .ExecuteDeleteAsync();   
        }
    }
}
