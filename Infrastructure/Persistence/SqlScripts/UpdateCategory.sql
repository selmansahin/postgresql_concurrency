-- Bu script, belirtilen ID'ye sahip kategoriyi doğrudan veritabanında günceller
-- Bu, xmin değerini değiştirecek ve API üzerinden yapılan isteklerde concurrency çakışması oluşturacaktır

-- Kategori ID'sini burada değiştirin (GUID formatında olmalı)
DO $$
DECLARE
    category_id UUID := 'KATEGORI_ID_BURAYA_YAZIN'; -- Örnek: '12345678-1234-1234-1234-123456789012'
BEGIN
    -- Kategoriyi güncelle
    UPDATE "Categories"
    SET "Name" = 'Veritabanından Güncellendi',
        "Description" = 'Bu kategori doğrudan veritabanı script''i ile güncellendi. ' || now(),
        "UpdatedAt" = now()
    WHERE "Id" = category_id;
    
    -- Sonuçları göster
    IF FOUND THEN
        RAISE NOTICE 'Kategori başarıyla güncellendi. xmin değeri değişti.';
        
        -- Güncellenmiş kategori bilgilerini ve xmin değerini göster
        RAISE NOTICE 'Güncellenmiş kategori bilgileri:';
        RAISE NOTICE '-----------------------------';
        PERFORM format('ID: %s, İsim: %s, Açıklama: %s, xmin: %s', 
                      "Id"::text, "Name", "Description", xmin::text)
        FROM "Categories"
        WHERE "Id" = category_id;
    ELSE
        RAISE NOTICE 'Belirtilen ID ile kategori bulunamadı: %', category_id;
    END IF;
END $$;
