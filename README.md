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
