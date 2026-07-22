Cách project này được tạo

## v1 — `QLBT/` (bản cũ, 1 client dùng chung cho SV/GV)

```
mkdir QLBT
cd QLBT

dotnet new sln -n QLBT
dotnet new classlib -n QLBT.Shared
dotnet new wpf -n QLBT.Server
dotnet new wpf -n QLBT.Client

dotnet sln add QLBT.Shared
dotnet sln add QLBT.Server
dotnet sln add QLBT.Client

dotnet add QLBT.Server reference QLBT.Shared
dotnet add QLBT.Client reference QLBT.Shared

dotnet add package WatsonTcp --version 5.1.7
dotnet add QLBT.Server package BCrypt.Net-Next --version 4.2.0
dotnet add QLBT.Server package Microsoft.Extensions.Configuration --version 8.0.0
dotnet add QLBT.Server package Microsoft.Extensions.Configuration.Json --version 8.0.1
dotnet add QLBT.Server package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.28
dotnet add QLBT.Server package Microsoft.EntityFrameworkCore.Design --version 8.0.28
```

## v2 — `QLBTv2/` (bản viết lại, tách riêng client cho Giáo viên và Sinh viên)

`QLBT.Server` giờ chỉ đóng vai trò host TCP + màn hình quản trị cục bộ (không còn kiêm luôn UI giáo viên). Nghiệp vụ giáo viên/sinh viên chuyển hẳn sang 2 app WPF riêng, dùng chung protocol/DTO trong `QLBT.Shared`.

```
mkdir QLBTv2
cd QLBTv2

dotnet new sln -n QLBTv2
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

# Giao thức TCP (dùng chung ở QLBT.Shared)
dotnet add QLBT.Shared package WatsonTcp --version 5.1.7

# CSDL + mật khẩu, chỉ QLBT.Server cần vì mọi truy vấn EF Core đều nằm ở đây
dotnet add QLBT.Server package BCrypt.Net-Next --version 4.2.0
dotnet add QLBT.Server package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.28
dotnet add QLBT.Server package Microsoft.EntityFrameworkCore.Design --version 8.0.28
dotnet add QLBT.Server package Microsoft.Extensions.Configuration --version 8.0.0
dotnet add QLBT.Server package Microsoft.Extensions.Configuration.Binder --version 8.0.2
dotnet add QLBT.Server package Microsoft.Extensions.Configuration.Json --version 8.0.1
```

Sau đó tạo CSDL và scaffold model (xem `PRN_conf.md` cho chi tiết lệnh `dotnet ef dbcontext scaffold`):

```
sqlcmd -S "(local)" -i QLBT.sql
sqlcmd -S "(local)" -i QLBT_SampleData.sql

cd QLBTv2/QLBT.Server
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

## Cấu trúc kết quả

```
QLBTv2/
  QLBTv2.slnx
  QLBT.Shared/           -- Command/Payload/Envelope, TCP client-server wrapper (WatsonTcp)
  QLBT.Server/            -- Host TCP + màn hình admin cục bộ (Học kỳ, Giáo viên, Lớp, Sinh viên)
  QLBT.Client.Teacher/     -- App WPF cho giáo viên (đăng nhập, bài tập, chấm điểm, xem lớp)
  QLBT.Client.Student/     -- App WPF cho sinh viên (đăng nhập, xem/nộp bài, xem điểm)
```

Chạy multi-startup (`Server + Teacher + Student` cùng lúc) bằng file `QLBTv2/QLBTv2.slnLaunch` đã cấu hình sẵn trong Visual Studio.

Xem thêm sơ đồ kiến trúc/luồng nghiệp vụ tại `report.md`.
