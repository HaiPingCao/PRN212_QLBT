# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

QLBT (Quản Lý Bài Tập — "Assignment Management") is a WPF client/server assignment-submission
system for a course, built for PRN212. Teachers manage classes/assignments through a WPF admin
app (`QLBT.Server`); students log in and submit files through a separate WPF client
(`QLBT.Client`). The two communicate over a custom TCP protocol built on WatsonTcp, with a shared
message/model layer in `QLBT.Shared`.

Solution file: `QLBT.slnx` (references all three projects). Target framework:
`net8.0-windows10.0.17763.0`, WPF (`UseWPF=true`), nullable enabled. **Windows-only** — this
cannot be built or run on Linux/macOS.

## Build / run

```powershell
dotnet build QLBT.slnx
dotnet run --project QLBT.Server   # launches the teacher-facing WPF admin app (also hosts the TCP server)
dotnet run --project QLBT.Client   # launches the student-facing WPF app
```

There are no test projects in this repo (`find . -iname "*test*"` turns up nothing) and no lint
config — don't invent test/lint commands.

`QLBT.Server/appsettings.json` holds the SQL Server connection string (`ConnectionStrings:Default`)
and TCP server defaults (`ServerSettings:IpAddress/Port/RootFolder`). It's copied to the output
dir on build. The DB is a SQL Server database named `PRN212_P_QLBT`; EF Core model classes were
scaffolded from it (`QLBT.Server/Models/*.cs`, `Prn212PQlbtContext`) — treat them as generated and
edit the DB schema, not the model file, if the shape needs to change (see `OnModelCreating` in
`Prn212PQlbtContext.cs` for the full FK/index mapping).

`QLBT.Client/appsettings.json` just holds the default `ServerSettings:IpAddress/Port` the login
screen pre-fills — the student can still edit host/port at login time, this only sets the default.
Run the server first (and click Start in `ServerManagement`) before logging in from the client.

## Architecture

Three projects, one direction of dependency: `QLBT.Client` and `QLBT.Server` both reference
`QLBT.Shared`; `Shared` references neither.

- **QLBT.Shared** — the wire contract. `Models/Command.cs` is the enum of every message type
  (`LOGIN`, `GET_ASSIGNMENT_LIST`, `SUBMIT_ASSIGNMENT`, `FILE_CHUNK`, ...). `Models/Envelope.cs`
  defines `Message` (request: `Command Type`, `Token`, `object? Data`) and `Response` (`Success`,
  `Error`, `object? Data`). `Models/Payload.cs` has the per-command DTOs (e.g.
  `LoginRequest`/`LoginResponse`, `AssignmentDto`, `SubmitAssignmentRequest`,
  `FileChunkData`/`FileEndData`). `Transport/TcpServer.cs` and `Transport/TcpClient.cs` wrap
  WatsonTcp and know nothing about business logic — the server exposes a single
  `Action<Guid, Message>? OnMessage` delegate that the server app wires up. `Transport/JsonHandle.cs`
  holds the shared `JsonSerializerOptions` (case-insensitive, string enum converter) — always
  serialize/deserialize `Message`/`Response`/`Data` payloads with these options, not defaults,
  since the client and server rely on identical (de)serialization behavior over the wire.

- **QLBT.Server** — composition root is `App.xaml.cs`, which builds one long-lived `ServerHost`
  (`Services/ServerHost.cs`) on startup and stops it on exit. `ServerHost` owns the single
  `Prn212PQlbtContext` DbContext and the three business services
  (`AssignmentService`, `SubmissionService`, `TeacherAssignmentService`), so WPF views
  (`Views/ServerManagement.xaml`, `Views/AssignmentView.xaml`) share them instead of creating
  their own. `ServerManagement` view starts/stops the TCP server and edits
  `appsettings.json`'s `ServerSettings` (persisted via `ServerHost.SaveSettings`); `AssignmentView`
  is CRUD for `bai_tap` (assignments) done directly through `TeacherAssignmentService` — it does
  **not** go over the TCP protocol, since it's the same process. `Handlers/MessageHandler.cs` is
  where TCP protocol commands are dispatched: `Handle()` requires a valid `Token` for every
  command except `LOGIN` (validated via `IAuthService.ValidateToken`), then switches on
  `Command` to call one of the three services and reply via `TcpServer.Reply`. File
  transfer (both problem-file download and submission upload/download) is chunked at 64KB
  (`MessageHandler.ChunkSize`) and base64-encoded inside `FileChunkData`/`FileChunkDownData`.

- **QLBT.Client** — `Services/AuthClientService`, `AssignmentClientService`,
  `SubmissionClientService` are thin wrappers that build a `Message`, call
  `TcpClient.SendAndWait`, and deserialize `Response.Data` (a `JsonElement`) into the expected DTO.
  `Services/FileReceiver.cs` handles the chunked-download side (`FILE_CHUNK_DOWN`/`FILE_END_DOWN`).
  The UI is `LoginWindow` (reads default host/port from `QLBT.Client/appsettings.json`, connects a
  `TcpClient`, and calls `AuthClientService.Login`) followed by `MainWindow` (assignment list →
  detail pane → download problem file / submit / download submission / delete submission). All
  client service calls are blocking (`TcpClient.SendAndWait` / `FileReceiver.Receive` poll
  synchronously with a timeout), so `MainWindow`/`LoginWindow` always push them onto a background
  thread with `Task.Run` from `async void` event handlers rather than calling them on the UI
  thread — keep that pattern for any new client-side network call.

Auth is a simple in-memory session map (`AuthService._sessions`, `token → studentId`) that resets
on server restart; passwords are BCrypt-hashed (`sv.MatKhau`) and checked with
`BCrypt.Net.BCrypt.Verify`. `AuthClientService.Logout` just clears the local token/name (called
from `MainWindow`'s logout button) — there is still no `LOGOUT` wire command, so the server-side
session in `AuthService._sessions` is never explicitly invalidated and only goes away on server
restart.

`GET_SUBMISSION_BY_ASSIGNMENT` (`Command`, `Payload.GetSubmissionByAssignmentRequest`,
`ISubmissionService.GetSubmissionByAssignment`) was added alongside the client UI — the original
protocol only supported `GET_SUBMISSION` by submission ID, which the client has no way to learn
without either just having submitted or persisting IDs locally. This command lets `MainWindow`
ask "does the current student have a submission for assignment N" whenever an assignment is
selected, independent of session history.

### Domain model (Vietnamese naming — kept as-is, don't translate to English mid-refactor)

`GiaoVien` (teacher, PK `Msgv`) → `Lop` (class, FK `Msgv`) → `BaiTap` (assignment, FK `LopId`,
composite unique key `(Id, LopId)`) → `BaiNop` (submission, composite FK `(BaiTapId, LopId)` →
`BaiTap`, composite FK `(LopId, Mssv)` → `SinhVienLop`). `SinhVien` (student, PK `Mssv`) enrolls in
classes via `SinhVienLop` (join table, composite PK `(LopId, Mssv)`, tracks `DangThamGia`/
`NgayThamGia`/`NgayRoiLop`). A `BaiNop` is unique per `(BaiTapId, Mssv)` — resubmission increments
`SoLanNop` rather than creating a new row.

Files live on disk under `ServerSettings:RootFolder` (default `Submissions`), not in the DB —
`BaiTap.DuongDanFileDe` / `BaiNop.DuongDanFile` store the path. Problem files are copied to
`{RootFolder}/ProblemFiles/{assignmentId}/{originalFileName}`
(`TeacherAssignmentService.CopyProblemFile`). Changing `RootFolder` via `ServerManagement` updates
`SubmissionService`/`TeacherAssignmentService` live, even while the TCP server itself is stopped.

## Known dead / incomplete code

Don't be surprised by these — they're existing gaps, not something to silently "fix" as a side
effect of an unrelated change:

- `AuthService.Logout` (server-side) has no caller — no `LOGOUT` command is dispatched anywhere,
  so server sessions never expire before restart. (`AuthClientService.Logout` on the client side
  *is* used — see above — but it's purely local and doesn't touch the server.)
- `Services/Crypt.cs` (`Crypt.Encrypt`/`Crypt.IsValid`) is unreferenced; `AuthService.Login` calls
  `BCrypt.Net.BCrypt.Verify` directly instead.
- `TcpClient.Send(string)` has no live caller in current source (`SendMessage(Message)` is what's
  actually used everywhere).
