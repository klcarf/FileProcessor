namespace Storage
{
    public interface IStorageProvider
    {
        public string Name { get; set; }
        Task AddChunkAsync(string key, byte[] data);
        Task<byte[]> GetChunkAsync(string key);
        Task DeleteChunkAsync(string key);
    }
}
