AGIT Technical Assessment — Slot Balancing System

Candidate Token: `VEH-GHALIBCANDIDATE`


## Teknologi dan Version

- **Framework**: .NET 8 (`net8.0`)
- **Language**: C# 12
- **Backend Web API**: ASP.NET Core Web API 8.0
- **Frontend UI**: Blazor Interactive Server (.NET 8)
- **Database & ORM**: Entity Framework Core 8.0.11 & Npgsql PostgreSQL Provider 8.0.11
- **API Documentation**: Swagger / OpenAPI (Swashbuckle 6.6.2)

### Langkah-langkah Menjalankan Aplikasi:

#### 1. Clone Repository
```bash
git clone <repository-url>
cd AGITnet
```

#### 2. Konfigurasi Database PostgreSQL
Buka `src/AGITnet.Api/appsettings.json` dan sesuaikan string koneksi PostgreSQL:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=agitnet_db;Username=postgres;Password=postgres"
}
```

#### 3. Menjalankan Migration Database
Gunakan CLI untuk menerapkan EF Core Migration ke database PostgreSQL:
```bash
dotnet ef database update --project src/AGITnet.Infrastructure --startup-project src/AGITnet.Api
```

#### 4. Menjalankan REST Backend API
```bash
dotnet run --project src/AGITnet.Api
```
- API akan aktif di: `http://localhost:5134`
- Swagger UI (Dokumentasi Interaktif): `http://localhost:5134/swagger`

#### 5. Menjalankan Blazor Frontend Web
Buka terminal terpisah dan jalankan:
```bash
dotnet run --project src/AGITnet.Web
```
- Aplikasi Web Blazor akan aktif di: `http://localhost:5274`

#### 6. Menjalankan Seluruh Testing (Unit & Integration Tests)
```bash
dotnet test AGITnet.slnx
```

## Tantangan Teknis Utama & Pengalaman Pengerjaan

Selama proses pengerjaan proyek ini, sebagian besar waktu pengembangan pada penanganan kasus-kasus teknis di bagian Backend API, EF Core, dan skrip SQL. Beberapa kendala utama yang saya hadapi dan solusi saya:

### 1. Kendala Backend API, Persistence, & Lingkungan Run (.NET 8 & EF Core)

* **Penanganan Race Condition pada Unique `RequestCode`**:
  Pengecekan sederhana menggunakan `if (GetByRequestCode != null)` di kode tidak cukup untuk menahan dua request HTTP yang masuk secara bersamaan di milidetik yang sama. Request kedua bisa menembus pengecekan dan memicu error `DbUpdateException` dari database. Agar API tidak *crash* dengan HTTP 500, saya menangkap `DbUpdateException` di level repository dan menerjemahkannya menjadi respon HTTP 409 Conflict.

* **Fleksibilitas Koneksi Database (PostgreSQL vs In-Memory Fallback)**:
  Agar aplikasi dapat dites langsung tanpa memaksakan penguji untuk menyiapkan database PostgreSQL terlebih dahulu, saya mengonfigurasi `Program.cs` agar melakukan `EnsureCreated()` ke PostgreSQL jika server aktif, dan secara otomatis *fallback* menggunakan database In-Memory jika PostgreSQL tidak ditemukan.

* **Isolasi Database pada Integration Testing (`WebApplicationFactory`)**:
  Saat membuat unit & integration test untuk API, EF Core In-Memory secara default dapat berbagi state antar request. Saya menambahkan kustomisasi pada `WebApplicationFactory` untuk menghapus registrasi DbContext bawaan dan mengisolasi database per-fixture test agar seluruh 28 test dapat berjalan 100% lulus tanpa saling mengganggu.

---

### 2. Kendala Migrasi Data Legacy pada SQL (PostgreSQL)

* **Unpivoting Struktur *Wide Columns* ke Tabel Normal (`database/assessment.sql`)**:
  Pada Task 10 SQL, tantangan terbesarnya adalah mengubah data dari tabel lama yang berbentuk kolom melebar (`slot1_qty`, `slot2_qty`, `slot3_qty`, ...) menjadi struktur tabel berelasi (*header-detail*). Mengubah kolom horizontal menjadi baris vertikal tanpa kehilangan relasi header dan tanpa membuat sisa data yang korup membutuhkan teknik `LATERAL (VALUES ...)` di PostgreSQL serta skrip rekonsiliasi total sebelum tabel lama aman untuk dihapus.

---

## Referensi Dokumentasi

- [Microsoft Learn — ASP.NET Core Web API Documentation](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [Microsoft Learn — Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Microsoft Learn — ASP.NET Core Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Microsoft Learn — Integration Tests in ASP.NET Core (`WebApplicationFactory`)](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [PostgreSQL Official Documentation — Data Definition & SELECT LATERAL](https://www.postgresql.org/docs/current/queries-table-expressions.html#QUERIES-LATERAL)
- [Npgsql Entity Framework Core Provider Documentation](https://www.npgsql.org/efcore/)
- [xUnit.net Official Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)
