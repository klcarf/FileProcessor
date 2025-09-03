
using Metadata;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Application.Interfaces
{
    public class MetadataService : IMetadataContract, IDisposable
    {
        private readonly FileProcessDbContext _context;

        public MetadataService(FileProcessDbContext context)
        {
            _context = context;
        }
        public async Task AddChunkAsync(Chunk chunk)
        {
            _context.Chunks.Add(chunk);
            await _context.SaveChangesAsync();
        }

        public Task<List<Chunk>> GetChunksAsync(Guid fileId) 
            => _context.Chunks.Where(c => c.FileId == fileId).OrderBy(c => c.Index).ToListAsync();

        public Task<Models.File?> GetFileAsync(Guid id) 
            => _context.Files.FindAsync(id).AsTask();

        public async Task<List<Models.File>> GetFilesAsync() 
            => await _context.Files.OrderByDescending(f => f.UpdateDate).ToListAsync();

        public async Task UpsertFileAsync(Models.File file)
        {
            var isExist = await _context.Files.FindAsync(file.Id);
            if (isExist == null)
            {
                _context.Files.Add(file);
            }
            else
            {
                _context.Files.Update(file);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task UpsertFolderAsync(Folder folder)
        {
            var isExist = await _context.Folders.FindAsync(folder.Id);
            if (isExist == null)
            {
                _context.Folders.Add(folder);
            }
            else
            {
                _context.Folders.Update(folder);
            }

            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        
    }
}
