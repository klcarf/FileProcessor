namespace Application
{
    public static class ChunkSizer
    {
        private const int FixedLargeChunkBytes = 100 * 1024 * 1024; 

        public static int GetChunkSize(long fileSizeBytes)
        {
            if (fileSizeBytes <= 0)
                return 1 * 1024 * 1024; 

            const long MB = 1024L * 1024L;

            if (fileSizeBytes <= 100 * MB)
            {
                return (int)Math.Ceiling(fileSizeBytes / 5.0);
            }
            else if (fileSizeBytes <= 1000 * MB)
            {
                return (int)Math.Ceiling(fileSizeBytes / 10.0);
            }
            else
            {
                return FixedLargeChunkBytes;
            }
        }
    }
}
