
namespace Storage
{
    public class LocalStorageProvider : IStorageProvider
    {
        public string Name { get; set; }
        private readonly string _root;

        public LocalStorageProvider(string name, string root)
        {
            Name = name;
            _root = root;
            Directory.CreateDirectory(_root);
        }

        public async Task AddChunkAsync(string key, byte[] data)
        {
            var path = Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar));
            DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.GetDirectoryName(path));
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await fileStream.WriteAsync(data.ToArray());
        }

        public Task DeleteChunkAsync(string key)
        {
            var path = Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar));

            if(File.Exists(path))
                File.Delete(path);

            return Task.CompletedTask;
        }

        public async Task<byte[]> GetChunkAsync(string key)
        {
            var path = Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            var buffer = new byte[fileStream.Length];
            int read = await fileStream.ReadAsync(buffer);

            if (read != buffer.Length)
                Array.Resize(ref buffer, read);

            return buffer;
        }
    }
}
