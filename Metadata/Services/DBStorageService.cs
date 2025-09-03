using Metadata.ServiceContracts;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata.Services
{
    public class DBStorageService : IDBStorageContract
    {

        private readonly FileProcessDbContext _context;

        public DBStorageService(FileProcessDbContext context)
        {
            _context = context;
        }
        public async Task DeleteStorageAsync(String fileId, int chunkOrder)
        {
            var chunkToDelete = await _context.DBStorages
            .FirstOrDefaultAsync(sc => sc.FileId == fileId && sc.ChunkOrder == chunkOrder);

            if (chunkToDelete != null)
            {
                _context.DBStorages.Remove(chunkToDelete);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<byte[]> ReadStorageDataAsync(String fileId, int chunkOrder)
        {
            var storage = await _context.DBStorages.FirstOrDefaultAsync(sc => sc.FileId == fileId && sc.ChunkOrder == chunkOrder);
            return storage?.ChunkData ?? Array.Empty<byte>();
        }

       

        public async Task SaveStorageAsync(String fileId, int chunkOrder, byte[] chunkData)
        {
            var newStorage = new DBStorage
            {
                FileId = fileId,
                ChunkOrder = chunkOrder,
                ChunkData = chunkData
            };

            _context.DBStorages.Add(newStorage);
            await _context.SaveChangesAsync();
        }

    }
}
