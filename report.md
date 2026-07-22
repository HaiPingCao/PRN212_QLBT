<!-- title: QLBT — Sơ đồ hệ thống -->

# QLBT — Sơ đồ kiến trúc & luồng nghiệp vụ

Hệ thống quản lý bài tập (QLBTv2): 3 ứng dụng WPF (`QLBT.Server`, `QLBT.Client.Teacher`, `QLBT.Client.Student`) giao tiếp qua TCP (WatsonTcp), dùng chung thư viện `QLBT.Shared`.

---

## 1. Actor & Use Case

```mermaid
flowchart LR
    subgraph Actors
        SV["👤 Sinh viên"]
        GV["👤 Giáo viên"]
        AD["👤 Quản trị viên<br/>(vận hành Server)"]
    end

    subgraph UC_SV["Use case — Sinh viên"]
        uc1((Đăng nhập))
        uc2((Xem danh sách bài tập))
        uc3((Xem chi tiết & tải đề))
        uc4((Nộp bài))
        uc5((Xem/xoá bài đã nộp))
        uc6((Xem điểm))
    end

    subgraph UC_GV["Use case — Giáo viên"]
        uc7((Đăng nhập))
        uc8((Xem lớp của tôi<br/>chỉ đọc))
        uc9((Quản lý bài tập<br/>CRUD))
        uc10((Chấm điểm bài nộp))
        uc11((Quản lý tài khoản SV<br/>Thêm/Sửa))
    end

    subgraph UC_AD["Use case — Admin (Server cục bộ)"]
        uc12((Cấu hình & bật/tắt Server))
        uc13((Quản lý Học kỳ))
        uc14((Quản lý Giáo viên))
        uc15((Quản lý Lớp & ghi danh))
        uc16((Quản lý tài khoản SV<br/>kể cả Xoá))
    end

    SV --- uc1 & uc2 & uc3 & uc4 & uc5 & uc6
    GV --- uc7 & uc8 & uc9 & uc10 & uc11
    AD --- uc12 & uc13 & uc14 & uc15 & uc16
```

**Ghi chú phân quyền:** Giáo viên **không còn** quyền xoá tài khoản sinh viên hay quản lý Lớp (CRUD) — hai việc này chỉ còn ở màn hình admin nội bộ của `QLBT.Server`, không đi qua TCP.

---

## 2. Sơ đồ xác thực (Screen Authorization)

```mermaid
flowchart TD
    Start(["Mở ứng dụng Client<br/>(Teacher hoặc Student)"]) --> Login["Màn hình Đăng nhập<br/>Nhập IP:Port + Tài khoản/Mật khẩu"]
    Login -->|"Command.Login<br/>Role = Teacher/Student"| Server{{"QLBT.Server<br/>AuthService"}}

    Server --> Check{"Đúng tài khoản?<br/>BCrypt verify theo bảng<br/>giao_vien / sinh_vien"}
    Check -->|Sai| Fail["Trả lỗi:<br/>'Sai tài khoản hoặc mật khẩu'"] --> Login
    Check -->|Đúng| Token["Sinh Token phiên<br/>lưu Dictionary&lt;token,(id, role)&gt; + lock"]

    Token --> Route{"Role?"}
    Route -->|Student| MainS["MainWindow — Student<br/>(mọi lệnh kèm Token)"]
    Route -->|Teacher| MainT["MainWindow — Teacher<br/>(mọi lệnh kèm Token)"]

    MainS -->|"mỗi Message gửi kèm Token"| Validate{{"ValidateToken(token)<br/>trong MessageHandler"}}
    MainT -->|"mỗi Message gửi kèm Token"| Validate

    Validate -->|Token sai/hết hạn| Deny["Reply: false<br/>'Invalid or expired token'"]
    Validate -->|Token hợp lệ| Guard{"Lệnh có<br/>RequireTeacher?"}
    Guard -->|"Có, role != Teacher"| DenyRole["Reply: false<br/>'Chỉ giáo viên mới được thực hiện'"]
    Guard -->|"Không hoặc role = Teacher"| Exec["Thực thi Service tương ứng"]

    style Server fill:#eef,stroke:#556
    style Validate fill:#eef,stroke:#556
```

`QLBT.Server`'s own admin views (Học kỳ / Giáo viên / Lớp / Sinh viên) chạy **cục bộ trong tiến trình Server**, gọi thẳng `IAdminService`/`IStudentService` — không qua bước xác thực Token/TCP ở trên vì không rời khỏi máy chủ.

---

## 3. Luồng màn hình (Screen Flow) theo từng app

```mermaid
flowchart LR
    subgraph Student["QLBT.Client.Student"]
        S1[LoginWindow] --> S2["MainWindow<br/>Danh sách bài tập<br/>(tìm kiếm)"]
        S2 -->|chọn 1 dòng| S3["Chi tiết bài tập<br/>Tải đề / Nộp bài / Xem điểm"]
        S3 -->|Nộp bài| S4["Chọn file → Upload theo chunk<br/>SubmitReady → FileChunk*N → FileEnd"]
        S4 --> S2
    end

    subgraph Teacher["QLBT.Client.Teacher"]
        T1[LoginWindow] --> T2["MainWindow (Tabs)"]
        T2 --> T3["Lớp của tôi<br/>(chỉ xem, tìm kiếm)"]
        T2 --> T4["Quản lý bài tập<br/>CRUD + đính kèm đề"]
        T2 --> T5["Chấm điểm<br/>chọn bài tập → DS bài nộp → nhập điểm"]
        T2 --> T6["Quản lý sinh viên<br/>Thêm/Sửa (không Xoá)"]
        T3 -->|chọn lớp| T3b["DS sinh viên trong lớp<br/>(read-only)"]
        T4 -->|chọn bài tập| T5
    end

    subgraph ServerApp["QLBT.Server (admin cục bộ)"]
        A1[MainWindow] --> A2["Cấu hình Server<br/>IP/Port, Start/Stop"]
        A1 --> A3["Quản lý Học kỳ"]
        A1 --> A4["Quản lý Giáo viên"]
        A1 --> A5["Quản lý Lớp<br/>checkbox thêm SV hàng loạt<br/>+ panel ghi danh (reactivate/remove)"]
        A1 --> A6["Quản lý Sinh viên<br/>Thêm/Sửa/Xoá"]
        A3 -.dữ liệu dùng bởi.-> A5
        A4 -.dữ liệu dùng bởi.-> A5
    end
```

---

## 4. Sơ đồ gọi hàm (Function Call Graph) — luồng "Nộp bài" & "Chấm điểm"

```mermaid
sequenceDiagram
    participant UI as StudentClient UI
    participant Svc as SubmissionClientService
    participant Tcp as TcpClient (Shared)
    participant MH as MessageHandler (Server)
    participant SS as SubmissionService
    participant DB as Prn212PQlbtContext

    UI->>Svc: Submit(assignmentId, filePath)
    Svc->>Tcp: SendAndWait(Command.SubmitAssignment)
    Tcp->>MH: Handle(clientId, msg)
    MH->>MH: HandleSubmitAssignment()
    MH->>SS: BeginUpload(assignmentId, studentId, fileName, size)
    SS-->>MH: uploadId
    MH-->>Tcp: Reply(SubmitReadyResponse)
    Tcp-->>Svc: uploadId

    loop mỗi chunk 64KB
        Svc->>Tcp: Send(Command.FileChunk)
        Tcp->>MH: HandleFileChunk()
        MH->>SS: AddChunk(uploadId, index, base64)
    end

    Svc->>Tcp: Send(Command.FileEnd)
    Tcp->>MH: HandleFileEnd()
    MH->>SS: FinalizeUpload(uploadId)
    SS->>DB: Insert/Update bai_nop (upsert theo uq_bai_nop)
    DB-->>SS: submissionId
    SS-->>MH: submissionId
    MH-->>Svc: SubmitOkResponse

    Note over UI,DB: --- Giáo viên chấm điểm ---
    participant TUI as TeacherClient UI
    TUI->>MH: Command.GetClassSubmissions(assignmentId)
    MH->>MH: RequireTeacher(role)
    MH->>SS: GetClassSubmissions(assignmentId, teacherId)
    SS->>DB: Query bai_nop join sinh_vien_lop
    DB-->>SS: List<SubmissionDto>
    SS-->>TUI: danh sách hiển thị DataGrid

    TUI->>MH: Command.GradeSubmission(submissionId, grade)
    MH->>MH: RequireTeacher(role)
    MH->>SS: SetGrade(submissionId, teacherId, grade)
    SS->>DB: UPDATE bai_nop SET diem = @grade
    DB-->>SS: ok
    SS-->>TUI: Reply(true)
```

---

## 5. Sơ đồ đa đồ thị gọi hàm (Function Call Multigraph) — `QLBT.Server`

Mọi `case` trong `MessageHandler.HandleInner` (đỉnh trái) gọi vào đúng một `Handle...` nội bộ, rồi xuống lớp Service tương ứng, rồi tới `Prn212PQlbtContext` (EF Core). Đây là đồ thị tĩnh toàn bộ lệnh TCP — nhiều cạnh cùng đổ vào một node Service (đa đồ thị), thể hiện việc tái sử dụng service giữa các lệnh khác nhau.

```mermaid
flowchart LR
    subgraph CMD["Command (Shared)"]
        direction TB
        cLogin[Login]
        cGetAsgList[GetAssignmentList]
        cGetAsgDetail[GetAssignmentDetail]
        cDownProblem[DownloadProblemFile]
        cSubmit[SubmitAssignment]
        cChunk[FileChunk]
        cEnd[FileEnd]
        cGetSub[GetSubmission]
        cDelSub[DeleteSubmission]
        cDownSub[DownloadSubmission]
        cGetClassList[GetClassList]
        cGetClassStu[GetClassStudents]
        cGetTAsg[GetTeacherAssignments]
        cCreateAsg[CreateAssignment]
        cUpdateAsg[UpdateAssignment]
        cDeleteAsg[DeleteAssignment]
        cGetClassSub[GetClassSubmissions]
        cGrade[GradeSubmission]
        cGetStuList[GetStudentList]
        cCreateStu[CreateStudent]
        cUpdateStu[UpdateStudent]
    end

    subgraph MH["MessageHandler"]
        direction TB
        hLogin[HandleLogin]
        hAsgDetail[HandleGetAssignmentDetail]
        hDownProblem[HandleDownloadProblemFile]
        hSubmit[HandleSubmitAssignment]
        hChunk[HandleFileChunk]
        hEnd[HandleFileEnd]
        hGetSub[HandleGetSubmission]
        hDelSub[HandleDeleteSubmission]
        hDownSub[HandleDownloadSubmission]
        hClassStu[HandleGetClassStudents]
        hTAsg[HandleGetTeacherAssignments]
        hCreateAsg[HandleCreateAssignment]
        hUpdateAsg[HandleUpdateAssignment]
        hDeleteAsg[HandleDeleteAssignment]
        hClassSub[HandleGetClassSubmissions]
        hGrade[HandleGradeSubmission]
        hCreateStu[HandleCreateStudent]
        hUpdateStu[HandleUpdateStudent]
        rTeacher{{RequireTeacher guard}}
        sendFile[[SendFileToClient]]
    end

    subgraph SVC["Service layer"]
        direction TB
        Auth["AuthService<br/>Login / ValidateToken"]
        Asg["AssignmentService<br/>GetAssignmentList<br/>GetAssignmentDetail<br/>GetProblemFilePath"]
        Sub["SubmissionService<br/>BeginUpload / AddChunk<br/>FinalizeUpload / SetGrade<br/>GetSubmissionByAssignment<br/>GetClassSubmissions<br/>GetSubmissionFilePath<br/>DeleteSubmission"]
        Cls["ClassService<br/>GetClassesOfTeacher<br/>GetClassStudents"]
        TAsg["TeacherAssignmentService<br/>GetAll / Create<br/>Update / Delete<br/>GetProblemFilePath"]
        Stu["StudentService<br/>GetAll / Add<br/>Update / Delete*"]
    end

    subgraph DB["Data layer"]
        EF[("Prn212PQlbtContext<br/>(EF Core / SQL Server)")]
    end

    cLogin --> hLogin --> Auth
    cGetAsgList --> Asg
    cGetAsgDetail --> hAsgDetail --> Asg
    cDownProblem --> hDownProblem --> Asg
    cDownProblem --> hDownProblem --> TAsg
    hDownProblem --> sendFile

    cSubmit --> hSubmit --> Sub
    cChunk --> hChunk --> Sub
    cEnd --> hEnd --> Sub
    cGetSub --> hGetSub --> Sub
    cDelSub --> hDelSub --> Sub
    cDownSub --> hDownSub --> Sub
    hDownSub --> sendFile

    cGetClassList --> rTeacher --> Cls
    cGetClassStu --> hClassStu --> rTeacher
    hClassStu --> Cls

    cGetTAsg --> hTAsg --> rTeacher
    hTAsg --> TAsg
    cCreateAsg --> hCreateAsg --> rTeacher
    hCreateAsg --> TAsg
    cUpdateAsg --> hUpdateAsg --> rTeacher
    hUpdateAsg --> TAsg
    cDeleteAsg --> hDeleteAsg --> rTeacher
    hDeleteAsg --> TAsg

    cGetClassSub --> hClassSub --> rTeacher
    hClassSub --> Sub
    cGrade --> hGrade --> rTeacher
    hGrade --> Sub

    cGetStuList --> rTeacher --> Stu
    cCreateStu --> hCreateStu --> rTeacher
    hCreateStu --> Stu
    cUpdateStu --> hUpdateStu --> rTeacher
    hUpdateStu --> Stu

    Auth --> EF
    Asg --> EF
    Sub --> EF
    Cls --> EF
    TAsg --> EF
    Stu --> EF

    style rTeacher fill:#fee,stroke:#a33
    style EF fill:#eef,stroke:#556
```

`Stu.Delete*` không còn đích gọi nào từ `MessageHandler` (đã gỡ khỏi TCP) — chỉ còn được gọi trực tiếp bởi `QLBT.Server.Views.StudentManagementView` (admin cục bộ), minh hoạ ở đồ thị dưới.

### 5b. Nhánh gọi cục bộ — Admin UI của `QLBT.Server` (không qua TCP)

```mermaid
flowchart LR
    subgraph Views["Server Views (code-behind)"]
        vHocKy[HocKyManagementView]
        vGV[TeacherManagementView]
        vLop[ClassManagementView]
        vSV[Server.StudentManagementView]
    end

    subgraph Adm["AdminService"]
        aHK["GetAllHocKy / AddHocKy<br/>UpdateHocKy / DeleteHocKy"]
        aGV["GetAllGiaoVien / AddGiaoVien<br/>UpdateGiaoVien / DeleteGiaoVien"]
        aLop["GetAllLop / AddLop<br/>UpdateLop / DeleteLop"]
        aEnr["GetClassStudents / GetAvailableStudents<br/>AddStudentsToClass<br/>RemoveStudentFromClass<br/>ReactivateStudentInClass"]
    end

    stuDel["StudentService.Delete(mssv)"]

    vHocKy --> aHK --> EF[("Prn212PQlbtContext")]
    vGV --> aGV --> EF
    vLop --> aLop --> EF
    vLop --> aEnr --> EF
    vSV --> stuDel --> EF
    vSV --> aGV
```

---

## 6. Sơ đồ lớp (Class Diagram) — Domain Model (EF Core)

```mermaid
classDiagram
    class GiaoVien {
        +string Msgv
        +string HoTen
        +string Email
        -string MatKhau
    }

    class SinhVien {
        +string Mssv
        +string HoTen
        +string Email
        -string MatKhau
    }

    class HocKi {
        +int Id
        +string TenHocKy
        +DateOnly? NgayBatDau
        +DateOnly? NgayKetThuc
    }

    class Lop {
        +int Id
        +string TenLop
        +string ChuyenNganh
        +getSoSinhVien() int
    }

    class SinhVienLop {
        +bool DangThamGia
        +DateTime NgayThamGia
        +DateTime? NgayRoiLop
    }

    class BaiTap {
        +int Id
        +string TieuDe
        +string? MoTa
        +string? TenFileDe
        +DateTime HanNop
        +DateTime NgayTao
        +isQuaHan() bool
    }

    class BaiNop {
        +int Id
        +string TenFile
        +int SoLanNop
        +DateTime NgayNop
        +decimal? Diem
        +isDaCham() bool
    }

    GiaoVien "1" --> "0..*" Lop : chủ nhiệm
    HocKi "1" --> "0..*" Lop : thuộc học kỳ
    Lop "1" --> "0..*" BaiTap : giao bài
    Lop "1" o-- "0..*" SinhVienLop : ghi danh
    SinhVien "1" o-- "0..*" SinhVienLop : tham gia
    BaiTap "1" --> "0..*" BaiNop : nhận bài nộp
    SinhVienLop "1" --> "0..1" BaiNop : nộp bài
```

**Ghi chú:**
- `SinhVienLop` là bảng trung gian (association class) giữa `Lop` và `SinhVien` — mang thêm thuộc tính riêng (`DangThamGia`, ngày tham gia/rời lớp) nên không thể gộp thành quan hệ nhiều-nhiều thẳng.
- Ràng buộc `uq_bai_nop (BaiTapId, Mssv)` khiến mỗi sinh viên chỉ có tối đa **1** bài nộp cho mỗi bài tập → quan hệ `SinhVienLop → BaiNop` là `0..1`, không phải nhiều.
- `MatKhau` đánh dấu `-` (private) vì chỉ dùng nội bộ để BCrypt-verify khi đăng nhập, không lộ ra DTO/API nào.
