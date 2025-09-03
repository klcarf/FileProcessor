namespace Models
{
    public class Storage
    {
        public Guid Id { get; set; }

        public String FileId { get; set; }

        public int ChunkOrder { get; set; }

        public byte[] ChunkData { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
