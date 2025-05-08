-- Bu script, veritabanındaki tüm kategorileri toplu olarak günceller
-- Bu, xmin değerlerini değiştirecek ve API üzerinden yapılan isteklerde concurrency çakışması oluşturacaktır

BEGIN;

-- Tüm kategorileri güncelle
UPDATE "Categories"
SET "Name" = "Name" || ' - Güncellendi',
    "Description" = "Description" || ' - Toplu güncelleme: ' || now(),
    "UpdatedAt" = now();

-- Güncellenen kategorileri ve xmin değerlerini göster
SELECT "Id", "Name", "Description", "RowVersion", xmin
FROM "Categories";

COMMIT;

-- NOT: Bu scripti çalıştırdıktan sonra, API üzerinden herhangi bir kategoriyi 
-- güncellemek istediğinizde, eski RowVersion değeri ile istek yaparsanız
-- concurrency çakışması hatası alacaksınız.
