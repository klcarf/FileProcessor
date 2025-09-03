namespace Application
{
    public static class Messages
    {
        public const string DownloadSuccess = $"Verification complete. Download successful.";
        public const string NotFound = $"Not found.";
        public const string UsageUpload = $"Usage: upload <filePath>";
        public const string UsageDownload = "Usage: download <fileId> <outputPath>";
        public const string UsageInfo = $"Usage: info <fileId>";
        public const string Help = @"DistStoreDemo commands:
                            upload <filePath> Uploads file by chunking & distributing
                            download <fileId> <outPath> Reassembles & verifies to outPath
                            info <fileId> Shows file & chunk metadata
                            list Lists all uploaded files";
    }
}
