namespace Storage
{
    public interface IStorageProvider
    {
        public string Name { get; set; }
        Task AddChunkAsync(string fileId, int chunkOrder, byte[] data);
        Task<byte[]> GetChunkAsync(string fileId, int chunkOrder);
        Task DeleteFileChunksAsync(string fileId);
    }
}
