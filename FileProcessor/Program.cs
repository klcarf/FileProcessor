using Application;
using Application.Interfaces;
using log4net;
using log4net.Config;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services.Exceptions;
using Spectre.Console;
using Storage;
using System.Reflection;

public class Program
{
    private readonly FileProcessorService _service;
    private readonly IMetadataContract _metadataContract;
    private readonly ILog _logger;

    public Program(FileProcessorService service, IMetadataContract metadataContract, ILog logger)
    {
        _service = service;
        _metadataContract = metadataContract;
        _logger = logger;
    }

    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var app = host.Services.GetRequiredService<Program>();
        await app.RunAsync();
    }

    public async Task RunAsync()
    {
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
                _logger.Error($"Error [{ex.ErrorCode}]: {ex.Message}");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Beklenmedik Hata: {ex.Message}[/]");
                _logger.Error($"Error: {ex.Message}", ex);
            }
        }
    }

    async Task ProcessCommandAsync(string cmd)
    {
        switch (cmd)
        {
            case "upload":
                var filePath = AnsiConsole.Ask<string>("Yüklenecek dosya veya klasörün [green]yolunu[/] girin:");
                var fileId = await _service.UploadAsync(filePath);
                AnsiConsole.MarkupLine($"[bold green]Yükleme başarılı.[/] FileId: [yellow]{fileId}[/]");
                _logger.Info($"Uploaded. FileId: {fileId}");
                break;

            case "download":
                var idToDownload = AnsiConsole.Ask<string>("İndirilecek dosyanın [green]FileId[/]'sini girin:");
                var outputPath = AnsiConsole.Ask<string>("Dosyanın kaydedileceği [green]hedef yolu[/] girin (örn: C:\\downloads\\restored.pdf):");
                Guid guid = Guid.Parse(idToDownload);
                await _service.DownloadAsync(guid, outputPath);
                AnsiConsole.MarkupLine("[bold green]Doğrulama tamamlandı. İndirme başarılı.[/]");
                _logger.Info("Verification complete. Download successful.");
                break;

            case "info":
                var idToInfo = AnsiConsole.Ask<string>("Bilgisi görüntülenecek dosyanın [green]FileId[/]'sini girin:");
                var infoId = Guid.Parse(idToInfo);
                var info = await _metadataContract.GetFileAsync(infoId);
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

                var chunks = await _metadataContract.GetChunksAsync(infoId);
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
                var files = await _metadataContract.GetFilesAsync();
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

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
                services.AddSingleton(LogManager.GetLogger(typeof(Program)));

                var fileProcessDbConnection = hostContext.Configuration.GetConnectionString("FileProcessDb");
                var dataBaseStorageDbConnection = hostContext.Configuration.GetConnectionString("DataBaseStorageDb");

                services.AddDbContext<FileProcessDbContext>(options =>
                    options.UseNpgsql(fileProcessDbConnection));

                services.AddDbContext<DatabaseStorageDbContext>(options =>
                    options.UseNpgsql(dataBaseStorageDbConnection));

                services.AddScoped<IMetadataContract, MetadataService>();

                services.AddSingleton<IStorageProvider>(
                    new LocalStorageProvider("A", Path.Combine(AppContext.BaseDirectory, "Storage", "ProviderA"))
                );

                services.AddScoped<IStorageProvider, DatabaseStorageProvider>(sp =>
                {
                    var dbContext = sp.GetRequiredService<DatabaseStorageDbContext>();
                    return new DatabaseStorageProvider("B", dbContext);
                });

                services.AddScoped<FileProcessorService>();
                services.AddScoped<Program>();
            });
}