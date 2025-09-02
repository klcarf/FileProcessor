
using FileProcessor;
using log4net;
using log4net.Config;
using Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;
using Services.Exceptions;
using Spectre.Console;
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

while (true)
{
    AnsiConsole.WriteLine();
    var command = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Hangi işlemi yapmak istersiniz?")
            .PageSize(10)
            .AddChoices(new[] {
                "upload", "download", "info", "list", "[red]exit[/]"
            }));

    if (command == "[red]exit[/]")
    {
        break;
    }

    try
    {
        await ProcessCommandAsync(command);
    }
    catch (FileProcessorException ex)
    {
        AnsiConsole.MarkupLine($"[red]Hata [{ex.ErrorCode}]: {ex.Message}[/]");
        logger.Error($"Error [{ex.ErrorCode}]: {ex.Message}");
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]Beklenmedik Hata: {ex.Message}[/]");
        logger.Error($"Error: {ex.Message}", ex);
    }
}


async Task ProcessCommandAsync(string cmd)
{
    switch (cmd)
    {
        case "upload":
            var filePath = AnsiConsole.Ask<string>("Yüklenecek dosya veya klasörün [green]yolunu[/] girin:");
            var fileId = await service.UploadAsync(filePath);
            AnsiConsole.MarkupLine($"[bold green]Yükleme başarılı.[/] FileId: [yellow]{fileId}[/]");
            logger.Info($"Uploaded. FileId: {fileId}");
            break;

        case "download":
            var idToDownload = AnsiConsole.Ask<string>("İndirilecek dosyanın [green]FileId[/]'sini girin:");
            var outputPath = AnsiConsole.Ask<string>("Dosyanın kaydedileceği [green]hedef yolu[/] girin (örn: C:\\downloads\\restored.pdf):");
            Guid guid = Guid.Parse(idToDownload);
            await service.DownloadAsync(guid, outputPath);
            AnsiConsole.MarkupLine("[bold green]Doğrulama tamamlandı. İndirme başarılı.[/]");
            logger.Info("Verification complete. Download successful.");
            break;

        case "info":
            var idToInfo = AnsiConsole.Ask<string>("Bilgisi görüntülenecek dosyanın [green]FileId[/]'sini girin:");
            var infoId = Guid.Parse(idToInfo);
            var info = await metadataContract.GetFileAsync(infoId);
            if (info == null) { AnsiConsole.MarkupLine("[red]Dosya bulunamadı.[/]"); return; }

            var fileDetailsTable = new Table().Border(TableBorder.Rounded);
            fileDetailsTable.AddColumn("Özellik");
            fileDetailsTable.AddColumn("Değer");
            fileDetailsTable.AddRow("FileId", $"[yellow]{info.Id}[/]");
            fileDetailsTable.AddRow("Dosya Adı", info.FileName);
            fileDetailsTable.AddRow("Boyut", $"{info.Size} bytes");
            fileDetailsTable.AddRow("Parça Sayısı", info.ChunkCount.ToString());
            fileDetailsTable.AddRow("SHA256 Hash", info.HashSha256);
            fileDetailsTable.AddRow("Güncelleme Tarihi", info.UpdateDate.ToString());
            AnsiConsole.Write(fileDetailsTable);

            var chunks = await metadataContract.GetChunksAsync(infoId);
            var chunksTable = new Table().Title("Dosya Parçaları (Chunks)").Border(TableBorder.Rounded);
            chunksTable.AddColumn("Index");
            chunksTable.AddColumn("Boyut");
            chunksTable.AddColumn("Provider:Key");
            chunksTable.AddColumn("SHA256 Hash");

            foreach (var chunk in chunks.OrderBy(c => c.Index))
            {
                chunksTable.AddRow($"[blue]{chunk.Index}[/]", $"{chunk.Size} bytes", $"{chunk.Provider}:{chunk.ProviderKey}", chunk.HashSha256);
            }
            AnsiConsole.Write(chunksTable);
            break;

        case "list":
            var files = await metadataContract.GetFilesAsync();
            var table = new Table().Title("Yüklenmiş Dosyalar").Border(TableBorder.Rounded);

            table.AddColumn("FileId");
            table.AddColumn("Dosya Adı");
            table.AddColumn("Boyut");
            table.AddColumn("Parça Sayısı");
            table.AddColumn("Güncelleme Tarihi");

            foreach (var file in files)
            {
                table.AddRow(
                    $"[yellow]{file.Id}[/]",
                    file.FileName,
                    $"{file.Size} bytes",
                    file.ChunkCount.ToString(),
                    file.UpdateDate.ToString()
                );
            }
            AnsiConsole.Write(table);
            break;
    }
}