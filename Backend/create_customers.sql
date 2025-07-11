CREATE TABLE Customers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TCKN TEXT NOT NULL UNIQUE CHECK(length(TCKN) = 11),
    Ad TEXT NOT NULL CHECK(length(Ad) <= 50),
    Soyad TEXT NOT NULL CHECK(length(Soyad) <= 50),
    DogumTarihi TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedDate TEXT NOT NULL DEFAULT (datetime('now'))
); 