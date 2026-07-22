```
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

dotnet add QLBT.Shared package WatsonTcp --version 5.1.7
dotnet add QLBT.Server package BCrypt.Net-Next --version 4.2.0
dotnet add QLBT.Server package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.28
dotnet add QLBT.Server package Microsoft.EntityFrameworkCore.Design --version 8.0.28
dotnet add QLBT.Server package Microsoft.Extensions.Configuration --version 8.0.0
dotnet add QLBT.Server package Microsoft.Extensions.Configuration.Binder --version 8.0.2
dotnet add QLBT.Server package Microsoft.Extensions.Configuration.Json --version 8.0.1
```

```
sqlcmd -S "(local)" -i QLBT.sql
sqlcmd -S "(local)" -i QLBT_SampleData.sql

cd QLBT/QLBT.Server
dotnet ef dbcontext scaffold "Server=(local);uid=sa;password=<mật khẩu>;database=PRN212_P_QLBT;Encrypt=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --force
```

Tạo `QLBT.Server/appsettings.json` (set `Copy to Output Directory = Always`):

```json
{
  "ConnectionStrings": {
    "Default": "Server=(local);uid=sa;password=<mật khẩu>;database=PRN212_P_QLBT;Encrypt=True;TrustServerCertificate=True;"
  },
  "ServerSettings": {
    "IpAddress": "0.0.0.0",
    "Port": 9000,
    "RootFolder": "Submissions"
  }
}
```
