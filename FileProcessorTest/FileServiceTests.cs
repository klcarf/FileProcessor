using Metadata.ServiceContracts;
using Models;
using Moq;
using Services;
using Services.Exceptions;
using Storage;

namespace FileProcessorTest
{
    public class FileServiceTests
    {
        private readonly Mock<IMetadataContract> _mockMetadataContract;
        private readonly Mock<IStorageProvider> _mockStorageProvider;
        private readonly FileProcessorService _fileService;
        private readonly List<IStorageProvider> _providers;

        public FileServiceTests()
        {
            _mockMetadataContract = new Mock<IMetadataContract>();
            _mockStorageProvider = new Mock<IStorageProvider>();
            _providers = new List<IStorageProvider> { _mockStorageProvider.Object };
            _fileService = new FileProcessorService(_mockMetadataContract.Object, _providers);
        }

        [Fact]
        public async Task UploadAsync_ShouldThrowFileProcessorException_WhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentFilePath = "non_existent_file.txt";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<FileProcessorException>(() => _fileService.UploadAsync(nonExistentFilePath));
            Assert.Equal(ErrorCodes.FileNotFound, exception.ErrorCode);
        }

        [Fact]
        public async Task UploadAsync_ShouldUploadFileSuccessfully()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            await System.IO.File.WriteAllTextAsync(tempFile, "test content");

            _mockMetadataContract.Setup(m => m.UpsertFileAsync(It.IsAny<Models.File>())).Returns(Task.CompletedTask);
            _mockMetadataContract.Setup(m => m.AddChunkAsync(It.IsAny<Chunk>())).Returns(Task.CompletedTask);
            _mockMetadataContract.Setup(m => m.SaveChanges()).ReturnsAsync(1);
            //_mockStorageProvider.Setup(p => p.AddChunkAsync(It.IsAny<string>(), It.IsAny<byte[]>())).Returns(Task.CompletedTask);


            // Act
            var fileId = await _fileService.UploadAsync(tempFile);

            // Assert
            Assert.NotEqual(Guid.Empty, fileId);
            _mockMetadataContract.Verify(m => m.UpsertFileAsync(It.IsAny<Models.File>()), Times.AtLeastOnce);
            _mockMetadataContract.Verify(m => m.AddChunkAsync(It.IsAny<Chunk>()), Times.AtLeastOnce);
            //_mockStorageProvider.Verify(p => p.AddChunkAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.AtLeastOnce);

            // Cleanup
            System.IO.File.Delete(tempFile);
        }


        [Fact]
        public async Task DownloadAsync_ShouldThrowInvalidOperationException_WhenFileDoesNotExist()
        {
            // Arrange
            var nonExistentFileId = Guid.NewGuid();
            _mockMetadataContract.Setup(m => m.GetFileAsync(nonExistentFileId)).ReturnsAsync((Models.File)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _fileService.DownloadAsync(nonExistentFileId, "any_path"));
        }

        [Fact]
        public async Task DownloadAsync_ShouldDownloadFileSuccessfully()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var tempOutput = Path.GetTempFileName();

            // 1. Test verisini ve hash'ini dinamik olarak oluşturalım
            var testContent = "Bu benim test içeriğimdir.";
            var testData = System.Text.Encoding.UTF8.GetBytes(testContent);
            var correctChunkHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(testData));

            // FileService'in tam dosya hash'ini de doğrulaması için bunu da hesaplayalım.
            // Tek bir chunk olduğu için chunk hash ile aynı olacak.
            var correctFileHash = correctChunkHash;

            // 2. Sahte nesneleri bu dinamik ve doğru verilerle oluşturalım
            var file = new Models.File
            {
                Id = fileId,
                ChunkCount = 1,
                HashSha256 = correctFileHash
            };
            var chunks = new List<Chunk>
    {
        new Chunk
        {
            Index = 0,
            HashSha256 = correctChunkHash,
            Provider = "A",
            ProviderKey = $"{fileId}/0"
        }
    };

            _mockMetadataContract.Setup(m => m.GetFileAsync(fileId)).ReturnsAsync(file);
            _mockMetadataContract.Setup(m => m.GetChunksAsync(fileId)).ReturnsAsync(chunks);
            _mockStorageProvider.Setup(p => p.Name).Returns("A");
            //_mockStorageProvider.Setup(p => p.GetChunkAsync(It.IsAny<string>())).ReturnsAsync(testData);

            // Act
            await _fileService.DownloadAsync(fileId, tempOutput);

            // Assert
            var downloadedContent = await System.IO.File.ReadAllTextAsync(tempOutput);
            Assert.Equal(testContent, downloadedContent);

            // Cleanup
            System.IO.File.Delete(tempOutput);
        }
    }
}

