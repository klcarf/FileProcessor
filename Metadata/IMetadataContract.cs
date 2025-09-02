using Microsoft.EntityFrameworkCore;
using Models;

namespace Metadata
{
    public interface IMetadataContract
    {
        Task UpsertFileAsync(Models.File file);
        Task<Models.File?> GetFileAsync(Guid id);
        Task<List<Models.File>> GetFilesAsync();
        Task AddChunkAsync(Chunk chunk);
        Task<List<Chunk>> GetChunksAsync(Guid fileId);
        Task<int> SaveChanges();
        Task UpsertFolderAsync(Folder folder);
    }
}
