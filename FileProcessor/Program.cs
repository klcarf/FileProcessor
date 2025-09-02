
using log4net;
using log4net.Config;
using Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;
using Services.Exceptions;
using Storage;
using System.Reflection;

var connectionString = "Host=localhost;Port=5432;Database=FileProcessDb;Username=postgres;Password=postgres;";

var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
ILog logger = LogManager.GetLogger(typeof(Program));


var host = CreateHostBuilder(args).Build();
var options = new DbContextOptionsBuilder<FileProcessDbContext>().UseNpgsql(connectionString).Options;
using var dataSource = new FileProcessDbContext(options);

IMetadataContract metadataContract = new MetadataService(dataSource);

static IHostBuilder CreateHostBuilder(string[] args) =>
     Host.CreateDefaultBuilder(args)
         .ConfigureServices((hostContext, services) =>
         {
             // TODO: appsettings.json'dan connection string'i al
             var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
             services.AddDbContext<FileProcessDbContext>(options =>
                 options.UseNpgsql(connectionString));

         });



var providers = new List<IStorageProvider>
{
    new LocalStorageProvider("A", Path.Combine(AppContext.BaseDirectory, "Storage","ProviderA")),
    new LocalStorageProvider("B", Path.Combine(AppContext.BaseDirectory, "Storage","ProviderB")),
};


var service = new FileService(metadataContract, providers);

//if (args.Length == 0)
//{
//    PrintHelp();
//    return;
//}

//var cmd = args[0].ToLowerInvariant();

var cmd = "download";
var path = "C:/Users/amine/AppData/Local/Temp/arif";
var id = "56c088ae-a020-4fdd-8c43-b50d9a641021";


try
{
    switch (cmd)
    {
        case "upload":
            //if (args.Length < 2) { Console.WriteLine("Usage: upload <filePath>"); return; }
            //var filePath = args[1];
            var fileId = await service.UploadAsync(path);
            Console.WriteLine($"Uploaded. FileId: {fileId}");
            logger.Info($"Uploaded. FileId: {fileId}");
            break;
        case "download":
            //if (args.Length < 3) { Console.WriteLine("Usage: download <fileId> <outputPath>"); return; }
            //var id = Guid.Parse(args[1]);
            var output = "C:/Users/amine/OneDrive/Masaüstü/resotred.pdf";
            Guid guid = new Guid(id);
            await service.DownloadAsync(guid, output);
            Console.WriteLine("Verification complete. Download successful.");
            logger.Info("Verification complete. Download successful.");
            break;
        case "info":
            if (args.Length < 2) { Console.WriteLine("Usage: info <fileId>"); return; }
            var infoId = Guid.Parse(args[1]);
            var info = await metadataContract.GetFileAsync(infoId);
            if (info == null) { Console.WriteLine("Not found."); return; }
            Console.WriteLine($"FileId: {info.Id} Name: {info.FileName} Size: {info.Size}" +
                              $" Chunks: {info.ChunkCount} SHA256: {info.HashSha256} Updated: {info.UpdateDate} ");
            logger.Info($"FileId: {info.Id} Name: {info.FileName} Size: {info.Size}" +
                              $" Chunks: {info.ChunkCount} SHA256: {info.HashSha256} Updated: {info.UpdateDate} ");
            var chunks = await metadataContract.GetChunksAsync(infoId);

            foreach(var chunk in chunks.OrderBy(c => c.Index))
            {
                Console.WriteLine($" [#{chunk.Index}] {chunk.Size} bytes | {chunk.Provider}:{chunk.ProviderKey} | {chunk.HashSha256}");
                logger.Info($" [#{chunk.Index}] {chunk.Size} bytes | {chunk.Provider}:{chunk.ProviderKey} | {chunk.HashSha256}");

            }
            break;
        case "list":
            var files = await metadataContract.GetFilesAsync();
            foreach(var file in files)
            {
                Console.WriteLine($"{file.Id} | {file.FileName} | {file.Size} bytes | {file.ChunkCount} chunks | {file.UpdateDate}");
                logger.Info($"{file.Id} | {file.FileName} | {file.Size} bytes | {file.ChunkCount} chunks | {file.UpdateDate}");

            }
            break;
        default:
            PrintHelp();
            break;
    }
}
catch(FileProcessorException ex)
{
    Console.WriteLine($"Error [{ex.ErrorCode}]: {ex.Message}");
    logger.Error($"Error [{ex.ErrorCode}]: {ex.Message}");

}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    logger.Error($"Error: {ex.Message}");
}


static void PrintHelp()
{
    Console.WriteLine(@"DistStoreDemo commands:
upload <filePath> Uploads file by chunking & distributing
download <fileId> <outPath> Reassembles & verifies to outPath
info <fileId> Shows file & chunk metadata
list Lists all uploaded files");
}