namespace Application
{
    public static class ChunkSizer
    {
        private const int MinChunkSizeBytes = 1 * 1024 * 1024; // 1 MB
        private const int MaxChunkSizeBytes = 16 * 1024 * 1024; // 16 MB

        private const double BaseMultiplier = 524288; // 512 KB

        public static int GetChunkSize(long fileSizeBytes)
        {
            if (fileSizeBytes <= 0)
            {
                return MinChunkSizeBytes;
            }
            // chunk size ı file boyutu arttıkça 16MB a yaklaştırıyoruz.
            double scale = Math.Log(fileSizeBytes, 10);
            double calculatedSize = BaseMultiplier * scale;

            // sınr kontrolü
            int finalSize = (int)Math.Clamp(calculatedSize, MinChunkSizeBytes, MaxChunkSizeBytes);

            return finalSize;
        }
    }
}
