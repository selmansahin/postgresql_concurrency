# PostgreSQL Concurrency Control POC

Bu proje, PostgreSQL'in xmin sistem sütununu kullanarak eşzamanlılık (concurrency) kontrolünün nasıl yapılacağını gösteren bir Proof of Concept (POC) uygulamasıdır.

## Proje Hakkında

Bu POC, aşağıdaki teknolojileri kullanmaktadır:

- .NET 8
- Entity Framework Core
- PostgreSQL
- FastEndpoints
- Vertical Slice Architecture

## Concurrency Kontrolü ve PostgreSQL xmin

PostgreSQL, her satır için otomatik olarak bir "xmin" sistem sütunu tutar. Bu sütun, satırı oluşturan veya en son güncelleyen transaction ID'sini içerir ve her güncelleme işleminde otomatik olarak değişir.

Bu projede, Entity Framework Core'un concurrency token özelliğini PostgreSQL'in xmin sütununa bağlayarak optimistic concurrency kontrolü sağlıyoruz. Bu sayede:

1. Bir entity okunduğunda, xmin değeri de alınır
2. Entity güncellenirken, xmin değeri kontrol edilir
3. Eğer xmin değeri değiştiyse (başka bir işlem tarafından güncellenmiş), concurrency çakışması tespit edilir
4. Çakışma durumunda, uygun bir hata mesajı döndürülür

## Proje Yapısı

Proje, Vertical Slice Architecture kullanılarak organize edilmiştir:

```
ConcurrencyApi/
├── Domain/
│   ├── Entities/
│   │   └── Category.cs
│   └── Exceptions/
│       └── ConcurrencyException.cs
├── Features/
│   └── Categories/
│       ├── Create/
│       ├── Update/
│       ├── Delete/
│       ├── Get/
│       └── GetById/
└── Infrastructure/
    └── Persistence/
        ├── Configurations/
        │   └── CategoryConfiguration.cs
        ├── SqlScripts/
        │   ├── UpdateCategory.sql
        │   └── UpdateMultipleCategories.sql
        └── ApplicationDbContext.cs
```

## Concurrency Kontrolü Nasıl Çalışır?

### Entity Konfigürasyonu

`CategoryConfiguration.cs` dosyasında, PostgreSQL'in xmin sütununu Entity Framework Core'a concurrency token olarak tanıtıyoruz:

```csharp
builder.Property(x => x.RowVersion)
    .HasColumnName("xmin")
    .HasColumnType("xid")
    .ValueGeneratedOnAddOrUpdate()
    .IsConcurrencyToken();
```

### Concurrency Çakışmalarını Yakalama

`ApplicationDbContext.cs` dosyasında, SaveChanges ve SaveChangesAsync metodlarını override ederek concurrency çakışmalarını yakalıyoruz:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    try
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        // Handle concurrency exception
        HandleConcurrencyException(ex);
        throw;
    }
}
```

### API Endpoint'lerinde Concurrency Kontrolü

Update ve Delete endpoint'lerinde, istek içindeki RowVersion değeri ile veritabanındaki mevcut RowVersion değerini karşılaştırarak concurrency kontrolü yapıyoruz:

```csharp
// Check if the RowVersion matches
if (category.RowVersion != req.RowVersion)
{
    // The entity has been modified since it was retrieved
    var problem = new ProblemDetails
    {
        Status = 409,
        Title = "Concurrency Conflict",
        Detail = "The entity has been modified by another process."
    };

    await SendAsync(problem, 409, ct);
    return;
}
```

## Concurrency Çakışması Test Senaryoları

### Senaryo 1: İki Eşzamanlı API İsteği

1. İstemci A, bir kategoriyi okur ve RowVersion=1 değerini alır
2. İstemci B, aynı kategoriyi okur ve RowVersion=1 değerini alır
3. İstemci A, kategoriyi günceller, RowVersion=2 olur
4. İstemci B, eski RowVersion=1 değeri ile güncelleme yapmaya çalışır
5. Sunucu, concurrency çakışması hatası döndürür

### Senaryo 2: API İsteği ve Doğrudan DB Güncelleme

1. İstemci, bir kategoriyi okur ve RowVersion=1 değerini alır
2. Veritabanında doğrudan SQL ile kategori güncellenir, xmin değeri değişir
3. İstemci, eski RowVersion=1 değeri ile güncelleme yapmaya çalışır
4. Sunucu, concurrency çakışması hatası döndürür

## SQL Test Scriptleri

`SqlScripts` klasöründe, concurrency çakışması oluşturmak için kullanabileceğiniz SQL scriptleri bulunmaktadır:

- `UpdateCategory.sql`: Belirli bir kategoriyi güncelleyerek xmin değerini değiştirir
- `UpdateMultipleCategories.sql`: Tüm kategorileri güncelleyerek xmin değerlerini değiştirir

## Nasıl Çalıştırılır?

1. PostgreSQL veritabanını kurun ve `appsettings.json` dosyasındaki bağlantı bilgilerini güncelleyin
2. Projeyi çalıştırın: `dotnet run`
3. Swagger UI üzerinden API'yi test edin: `https://localhost:5001/swagger`
4. Concurrency çakışması testleri için SQL scriptlerini kullanın

## Notlar

- Bu proje, production ortamı için değil, concurrency kontrolünün nasıl çalıştığını göstermek için tasarlanmıştır
- PostgreSQL'in xmin sütunu, her ne kadar concurrency kontrolü için kullanılabilse de, PostgreSQL dokümantasyonunda bu kullanım için resmi bir destek bulunmamaktadır
- Gerçek uygulamalarda, daha güvenilir concurrency kontrolü mekanizmaları kullanmayı düşünebilirsiniz
