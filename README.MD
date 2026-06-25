Cách project này được tạo

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
