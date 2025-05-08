# PostgreSQL Concurrency Control POC - Geliştirme Özeti

**Tarih:** 9 Mayıs 2025

## Proje Genel Bakış

Bu proje, PostgreSQL'in xmin sistem sütununu kullanarak eşzamanlılık (concurrency) kontrolünün nasıl yapılacağını gösteren bir Proof of Concept (POC) uygulamasıdır. Kategori varlıkları üzerinde eşzamanlı güncellemeler hem API üzerinden hem de veritabanı script'leri ile gerçekleştirilmekte ve bu işlemlerde version kontrolü yapılmaktadır.

## Kullanılan Teknolojiler

- .NET 8
- Entity Framework Core
- PostgreSQL
- FastEndpoints
- Vertical Slice Architecture

## Veritabanı Bağlantı Bilgisi

```json
"PostgreSQL": "Host=localhost;Port=5432;Database=concurrency_db;Username=postgres;Password=qasx7865"
```

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

## Önemli Bileşenler ve İşlevleri

### 1. Domain Katmanı

#### Category Entity (Domain/Entities/Category.cs)
- Temel entity sınıfı
- RowVersion özelliği, PostgreSQL'in xmin sütunuyla eşleştirilmiştir
- Id, Name, Description, CreatedAt, UpdatedAt alanlarını içerir

#### ConcurrencyException (Domain/Exceptions/ConcurrencyException.cs)
- Concurrency çakışmalarını yönetmek için özel exception sınıfı
- Çakışan entity'lerin bilgilerini saklar

### 2. Infrastructure Katmanı

#### ApplicationDbContext (Infrastructure/Persistence/ApplicationDbContext.cs)
- Entity Framework Core DbContext sınıfı
- SaveChanges ve SaveChangesAsync metodları override edilmiş
- DbUpdateConcurrencyException'ları yakalayıp özel ConcurrencyException'a dönüştürür

#### CategoryConfiguration (Infrastructure/Persistence/Configurations/CategoryConfiguration.cs)
- Category entity'sinin veritabanı yapılandırması
- xmin sütununu RowVersion ile eşleştirir:
  ```csharp
  builder.Property(x => x.RowVersion)
      .HasColumnName("xmin")
      .HasColumnType("xid")
      .ValueGeneratedOnAddOrUpdate()
      .IsConcurrencyToken();
  ```

### 3. Features Katmanı (Vertical Slice Architecture)

#### Create Endpoint
- Yeni kategori oluşturur
- Oluşturulan kategorinin RowVersion değerini döndürür

#### Update Endpoint
- Kategori güncellemesi yapar
- RowVersion kontrolü ile concurrency çakışmalarını tespit eder
- Çakışma durumunda 409 Conflict hatası döndürür

#### Delete Endpoint
- Kategori silme işlemi yapar
- RowVersion kontrolü ile concurrency çakışmalarını tespit eder
- Çakışma durumunda 409 Conflict hatası döndürür

#### Get Endpoint
- Tüm kategorileri listeler

#### GetById Endpoint
- ID'ye göre kategori getirir

### 4. SQL Test Scriptleri

#### UpdateCategory.sql
- Belirli bir kategoriyi doğrudan veritabanında günceller
- xmin değerini değiştirir
- API üzerinden yapılan isteklerde concurrency çakışması oluşturur

#### UpdateMultipleCategories.sql
- Tüm kategorileri toplu olarak günceller
- xmin değerlerini değiştirir
- API üzerinden yapılan isteklerde concurrency çakışması oluşturur

## Concurrency Kontrolü Nasıl Çalışır?

1. PostgreSQL'in xmin sistem sütunu, her satır için otomatik olarak transaction ID'sini tutar
2. Bu sütun, her güncelleme işleminde otomatik olarak değişir
3. EF Core, xmin sütununu concurrency token olarak kullanır
4. Update/Delete işlemlerinde, istek içindeki RowVersion ile veritabanındaki mevcut RowVersion karşılaştırılır
5. Eğer değerler eşleşmezse, başka bir işlem tarafından güncellenmiş demektir ve concurrency çakışması oluşur
6. Çakışma durumunda, 409 Conflict hatası döndürülür

## Test Senaryoları

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

## Yapılacak İşler ve Geliştirmeler

- [ ] Swagger arayüzü üzerinden API'yi test et
- [ ] Concurrency çakışması durumunda client tarafında nasıl yönetileceğine dair örnekler ekle
- [ ] Birim testleri ekle
- [ ] Entegrasyon testleri ekle
- [ ] Performans testleri ekle

## Notlar ve Uyarılar

- PostgreSQL'in xmin sütunu, her ne kadar concurrency kontrolü için kullanılabilse de, PostgreSQL dokümantasyonunda bu kullanım için resmi bir destek bulunmamaktadır
