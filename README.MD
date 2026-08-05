# QLBT - QUẢN LÝ BÀI TẬP

- Dự án môn PRN212 (Basis Cross-Platform Application Programming With .NET)
- Build bằng .NET 8 và Visual studio 22

### 1. Cách .Net solution được tạo ra.

```bash
dotnet new sln -n QLBT
dotnet new classlib -n QLBT.Shared
dotnet new wpf -n QLBT.Server
dotnet new wpf -n QLBT.Client.Teacher
dotnet new wpf -n QLBT.Client.Student

dotnet sln add QLBT.Shared
dotnet sln add QLBT.Server
dotnet sln add QLBT.Client.Teacher
dotnet sln add QLBT.Client.Student

dotnet add QLBT.Server reference QLBT.Shared
dotnet add QLBT.Client.Teacher reference QLBT.Shared
dotnet add QLBT.Client.Student reference QLBT.Shared
```

### 2. Cài các gói phụ thuộc (dependency).

```bash
dotnet add QLBT.Shared package WatsonTcp --version 5.1.7
dotnet add QLBT.Server package BCrypt.Net-Next --version 4.2.0
dotnet add QLBT.Server package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.28
dotnet add QLBT.Server package Microsoft.EntityFrameworkCore.Design --version 8.0.28
dotnet add QLBT.Server package Microsoft.Extensions.Configuration --version 8.0.0
dotnet add QLBT.Server package Microsoft.Extensions.Configuration.Binder --version 8.0.2
dotnet add QLBT.Server package Microsoft.Extensions.Configuration.Json --version 8.0.1
```

### 3. Tạo cơ sở dữ liệu và thêm bộ dữ liệu mẫu cho dự án.

```bash
sqlcmd -S "(local)" -i QLBT.sql
sqlcmd -S "(local)" -i QLBT_SampleData.sql
```

### 4. Tạo model dựa trên CSDL bằng EF.

```bash
cd QLBT/QLBT.Server
dotnet ef dbcontext scaffold "Server=(local);uid=sa;password=<mật khẩu>;database=PRN212_P_QLBT;Encrypt=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --force
```

Tạo `QLBT.Server/appsettings.json` (set `Copy to Output Directory = Always`):

```json
{
  "ConnectionStrings": {
    "Default": "Server=(local);uid=sa;password=your_password;database=PRN212_P_QLBT;Encrypt=True;TrustServerCertificate=True;"
  },
  "ServerSettings": {
    "IpAddress": "0.0.0.0",
    "Port": 9000,
    "RootFolder": "Submissions"
  }
}
```

Trong `QLBT.Server/Models/Prn212PQlbtContext.cs`, thay đổi `OnConfiguring()` thành như sau:

```c#
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    var builder = new ConfigurationBuilder();
    builder.SetBasePath(Directory.GetCurrentDirectory());
    builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    var configuration = builder.Build();
    optionsBuilder.UseSqlServer(configuration.GetConnectionString("Default"));
}
```
