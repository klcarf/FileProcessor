
namespace Storage
{
    public class LocalStorageProvider : IStorageProvider
    {
        public string Name { get; set; }
        private readonly string _root; // Örn: "C:/Storage"

        public LocalStorageProvider(string name, string root)
        {
            Name = name;
            _root = root;
            Directory.CreateDirectory(_root);
        }

        private string GetChunkPath(string fileId, int chunkOrder)
        {
            var fileDirectory = Path.Combine(_root, fileId);
            return Path.Combine(fileDirectory, $"{chunkOrder}.chunk");
        }

        public async Task AddChunkAsync(string fileId, int chunkOrder, byte[] data)
        {
            var path = GetChunkPath(fileId, chunkOrder);
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            await File.WriteAllBytesAsync(path, data);
        }

        public async Task<byte[]> GetChunkAsync(string fileId, int chunkOrder)
        {
            var path = GetChunkPath(fileId, chunkOrder);
            if (File.Exists(path))
            {
                return await File.ReadAllBytesAsync(path);
            }
            return null;
        }

        public Task DeleteFileChunksAsync(string fileId)
        {
            var fileDirectory = Path.Combine(_root, fileId);

            // Eğer dosyaya ait klasör varsa, içindeki tüm chunk'larla birlikte sil.
            if (Directory.Exists(fileDirectory))
            {
                Directory.Delete(fileDirectory, recursive: true);
            }

            return Task.CompletedTask;
        }
    }
}
