
FileProcessor 

Bu proje, büyük dosyaları ve klasörleri verimli bir şekilde işlemek, parçalara ayırmak (chunking) ve bu parçaları birden fazla depolama sağlayıcısına (yerel disk, veritabanı vb.) dağıtmak için geliştirilmiş bir .NET konsol uygulamasıdır. Dosya bütünlüğü, her bir parça ve dosyanın tamamı için SHA256 hash'leri kullanılarak garanti altına alınır.

🚀 Temel Özellikler
- Dosya ve Klasör Yükleme: Tek bir dosyayı veya bir klasörün içindeki tüm dosyaları yükler.
  - C:\Users\Test\Masaüstü\Test           
  - C:\Users\Test\Masaüstü\Test\test.pdf  
- Dinamik Parçalama (Chunking): Dosyaları, boyutlarına göre logaritmik olarak hesaplanan yönetilebilir parçalara böler.
- Dağıtık Depolama: Oluşturulan parçaları, tanımlanmış farklı depolama sağlayıcıları (IStorageProvider) arasında dağıtır.
- Proje, LocalStorageProvider (yerel disk:..FileProcessor\bin\Debug\net8.0\Storage) ve DatabaseStorageProvider (veritabanı) implementasyonlarını içerir.
- Veri Bütünlüğü: Yükleme sırasında her bir parça ve dosyanın tamamı için SHA256 hash'leri oluşturulur. İndirme sırasında bu hash'ler doğrulanarak verinin bozulmadığından emin olunur.
- Metadata Yönetimi: Tüm dosya (File), klasör (Folder) ve parça (Chunk) bilgileri, ilişkisel bir veritabanında (PostgreSQL) saklanır.
- Etkileşimli Konsol Arayüzü: Spectre.Console kütüphanesi ile geliştirilmiş, kullanıcı dostu bir komut satırı arayüzü sunar.

🛠️ Kullanılan Teknolojiler
- .NET 8
- Entity Framework Core: Veritabanı işlemleri için ORM.
- PostgreSQL: Veritabanı.
- Docker: Veritabanı ortamını kolayca kurmak için.
- log4net: Loglama altyapısı.
  ..\FileProcessor\bin\Debug\net8.0\Logs
- Spectre.Console: Konsol arayüzü oluşturmak için.


Gereksinimler
- .NET 8 SDK
- Docker Desktop

Kurulum ve Çalıştırma
- Proje, iki adet PostgreSQL veritabanını Docker üzerinde otomatik olarak kurar. Terminali projenin ana klasöründe (.sln dosyasının olduğu yerde) açın ve aşağıdaki komutu çalıştırın:
> docker-compose up -d
- Bu komut, FileProcessDb ve DatabaseStorageDb adında iki veritabanı container'ı oluşturup arka planda başlatacaktır.

Veritabanı Şemalarını Oluşturma
Veritabanları başlatıldıktan sonra, gerekli tabloları oluşturmak için Entity Framework Core migration'larını uygulamanız gerekir. Terminalin hala projenin ana klasöründe olduğundan emin olun ve aşağıdaki komutları sırayla çalıştırın:

FileProcessDb Veritabanı için:
> > dotnet ef migrations add FileProcessDb_v1.0 --project Metadata --startup-project FileProcessor --context FileProcessDbContext
> 
> > dotnet ef database update --project Metadata --startup-project FileProcessor --context FileProcessDbContext

DatabaseStorageDb Veritabanı için:
> > dotnet ef migrations add DatabaseStorageDbContext_v1.0 --project Metadata --startup-project FileProcessor --context DatabaseStorageDbContext
> 
> > dotnet ef database update --project Metadata --startup-project FileProcessor --context DatabaseStorageDbContext 

💻 Kullanım
Uygulama başladığında, size hangi işlemi yapmak istediğinizi soran bir menü sunacaktır:
- upload: Bir dosya veya klasörün tam yolunu girerek sisteme yüklemenizi sağlar.
- download: Daha önce yüklenmiş bir dosyanın FileId'sini ve hedef kayıt yolunu girerek dosyayı indirmenizi sağlar.
- info: Bir FileId girerek o dosyaya ait tüm meta verileri ve parçalarının nerede saklandığını listeler.
- list: Sisteme yüklenmiş tüm dosyaları listeler.
- exit: Uygulamadan çıkar.
