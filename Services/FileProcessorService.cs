using FileProcessorUtil;
using Models;
using Services.Exceptions;
using Storage;
using System.Security.Cryptography;
using log4net;
using Metadata.ServiceContracts;

namespace Services
{
    public class FileProcessorService
    {
        private readonly IMetadataContract _metadataContract;
        private readonly List<IStorageProvider> _providers;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FileProcessorService));

        public FileProcessorService(IMetadataContract metadataContract, List<IStorageProvider> providers)
        {
            if (providers == null || providers.Count == 0)
            {
                throw new FileProcessorException(ErrorCodes.ProviderRequired, "Provider is required.");
            }

            _metadataContract = metadataContract;
            _providers = providers;
        }


        public async Task<Guid> UploadAsync(string filePath)
        {
            Folder? folder = null;

            if (Directory.Exists(filePath))
            {
                folder = new Folder
                {
                    Id = Guid.NewGuid(),
                    RootPath = Path.GetFullPath(filePath),
                    RootName = Path.GetFileName(Path.GetFullPath(filePath)
                                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                    CreateDate = DateTime.UtcNow
                };

                await _metadataContract.UpsertFolderAsync(folder);

                int count = 0;
                foreach (var file in Directory.EnumerateFiles(filePath, "*", SearchOption.AllDirectories))
                {
                    var id = await createFile(file, folder.Id);
                    var rel = Path.GetRelativePath(filePath, file).Replace('\\', '/');
                    Console.WriteLine($"Uploaded: {rel}  |  FileId: {id}");
                    _logger.Info($"Uploaded: {rel}  |  FileId: {id}");
                    count++;
                }
                Console.WriteLine($"Folder upload finished. Total files: {count}");
                _logger.Info($"Folder upload finished. Total files: {count}");
                await _metadataContract.SaveChanges();
                return folder.Id;
            }

            if (!System.IO.File.Exists(filePath))
                throw new FileProcessorException(ErrorCodes.FileNotFound, "File not found: " + filePath);

            return await createFile(filePath, null);
        }

        private async Task<Guid> createFile(string filePath, Guid? folderId)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            int chunkSize = ChunkSizer.GetChunkSize(fileStream.Length);
            var buffer = new byte[chunkSize];
            int providerPointer = 0;

            (Guid fileId, int chunkIndex, long totalBytes, Models.File file) = await addFile(filePath, folderId);


            var result = await chunkProcesses(fileStream, hasher, buffer, providerPointer, fileId, chunkIndex, chunkSize, totalChunkToSave: 50);
            providerPointer = result.providerPointer;
            chunkIndex = result.chunkIndex;
            long totalRead = result.totalBytes;

            var fullHash = Convert.ToHexString(hasher.GetHashAndReset());

            file.Size = totalRead;
            file.ChunkCount = chunkIndex;
            file.HashSha256 = fullHash;
            file.UpdateDate = DateTime.UtcNow;

            await _metadataContract.SaveChanges();

            return fileId;
        }

        private async Task<(int providerPointer, int chunkIndex, long totalBytes)> chunkProcesses(
            FileStream fileStream,
            IncrementalHash hasher,
            byte[] buffer,
            int providerPointer,
            Guid fileId,
            int chunkIndex,
            int chunkSize,
            int totalChunkToSave = 50)
        {
            long totalBytes = 0;
            int sinceLastSave = 0;

            while (true)
            {
                int read = await fileStream.ReadAsync(buffer.AsMemory(0, chunkSize));
                if (read <= 0) break;

                var slice = new byte[read];
                Buffer.BlockCopy(buffer, 0, slice, 0, read);

                var chunkHash = Convert.ToHexString(SHA256.HashData(slice));

                var provider = _providers[providerPointer];
                providerPointer = (providerPointer + 1) % _providers.Count; // Todo

                var providerKey = $"{fileId}/{chunkIndex}";
                await provider.AddChunkAsync(fileId.ToString(), chunkIndex, slice);

                var chunk = new Chunk
                {
                    Id = Guid.NewGuid(),
                    FileId = fileId,
                    Index = chunkIndex,
                    Size = read,
                    HashSha256 = chunkHash,
                    Provider = provider.Name,
                    ProviderKey = providerKey
                };

                await _metadataContract.AddChunkAsync(chunk);

                hasher.AppendData(slice);

                totalBytes += read;
                chunkIndex++;
                sinceLastSave++;

                if (sinceLastSave >= totalChunkToSave)
                {
                    await _metadataContract.SaveChanges();
                    sinceLastSave = 0;
                }
            }

            if (sinceLastSave > 0)
                await _metadataContract.SaveChanges();

            return (providerPointer, chunkIndex, totalBytes);
        }

        private async Task<(Guid fileId, int chunkIndex, long totalBytes, Models.File file)> addFile(string filePath, Guid? folderId)
        {
            var fileId = Guid.NewGuid();
            var fileName = Path.GetFileName(filePath);

            var file = new Models.File
            {
                Id = fileId,
                FileName = fileName,
                Size = 0,
                ChunkCount = 0,
                FolderId = folderId,           // <-- nullable destek
                UpdateDate = DateTime.UtcNow
            };

            await _metadataContract.UpsertFileAsync(file);
            await _metadataContract.SaveChanges();

            return (fileId, 0, 0, file);
        }

        public async Task DownloadAsync(Guid fileId, string outputPath)
        {
            var file = await _metadataContract.GetFileAsync(fileId) ??
                throw new InvalidOperationException("File not found.");
            var chunks = (await _metadataContract.GetChunksAsync(fileId)).OrderBy(c => c.Index).ToList();
            if (chunks.Count != file.ChunkCount)
            {
                throw new FileProcessorException(ErrorCodes.ChunkCountMismatch, "Chunk count not valid");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            using var outStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var fullHasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);


            foreach (var chunk in chunks)
            {
                var provider = _providers.FirstOrDefault(p => p.Name == chunk.Provider) ??
                    throw new FileProcessorException(ErrorCodes.ProviderNotFound, "Provider not found: " + chunk.Provider);

                var data = await provider.GetChunkAsync(fileId.ToString() ,chunk.Index);

                var currentChunkHash = Convert.ToHexString(SHA256.HashData(data));
                if (!currentChunkHash.Equals(chunk.HashSha256, StringComparison.OrdinalIgnoreCase))
                    throw new FileProcessorException(ErrorCodes.ChunkHashMismatch, "Chunk hash mismatch " + chunk.Index);

                await outStream.WriteAsync(data);
                fullHasher.AppendData(data);
            }

            var currentFullHash = Convert.ToHexString(fullHasher.GetHashAndReset());
            if (!currentFullHash.Equals(file.HashSha256, StringComparison.OrdinalIgnoreCase))
                throw new FileProcessorException(ErrorCodes.FileHashMismatch, "File hash mismatch.");
        }
    }
}
