using QLBT.Server.Services;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.IO;
using System.Text.Json;

namespace QLBT.Server.Handlers
{
    public class MessageHandler
    {
        private readonly TcpServer _server;
        private readonly IAuthService _auth;
        private readonly IAssignmentService _assignment;
        private readonly ISubmissionService _submission;
        private readonly IClassService _class;
        private readonly ITeacherAssignmentService _teacherAssignment;
        private readonly IStudentService _student;

        private const int ChunkSize = 64 * 1024; // 64KB per chunk

        public MessageHandler(
            TcpServer server,
            IAuthService auth,
            IAssignmentService assignment,
            ISubmissionService submission,
            IClassService classService,
            ITeacherAssignmentService teacherAssignment,
            IStudentService studentService)
        {
            _server = server;
            _auth = auth;
            _assignment = assignment;
            _submission = submission;
            _class = classService;
            _teacherAssignment = teacherAssignment;
            _student = studentService;

            _server.OnMessage = Handle;
        }

        private void Handle(Guid clientId, Message msg)
        {
            try
            {
                HandleInner(clientId, msg);
            }
            catch (Exception ex)
            {
                // Bat loi chung (thuong do CSDL khong ket noi duoc) de tra loi ro rang thay vi client bi treo/hieu nham.
                _server.Reply(clientId, false, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        private void HandleInner(Guid clientId, Message msg)
        {
            if (msg.Type == Command.Login)
            {
                HandleLogin(clientId, msg);
                return;
            }

            var session = _auth.ValidateToken(msg.Token);
            if (session == null)
            {
                _server.Reply(clientId, false, "Invalid or expired token");
                return;
            }

            var (userId, role) = session.Value;

            switch (msg.Type)
            {
                // Student-facing
                case Command.GetAssignmentList:
                    _server.Reply(clientId, true, data: _assignment.GetAssignmentList(userId));
                    break;

                case Command.GetAssignmentDetail:
                    HandleGetAssignmentDetail(clientId, msg, userId);
                    break;

                case Command.DownloadProblemFile:
                    HandleDownloadProblemFile(clientId, msg, userId, role);
                    break;

                case Command.SubmitAssignment:
                    HandleSubmitAssignment(clientId, msg, userId);
                    break;

                case Command.FileChunk:
                    HandleFileChunk(clientId, msg);
                    break;

                case Command.FileEnd:
                    HandleFileEnd(clientId, msg);
                    break;

                case Command.GetSubmission:
                    HandleGetSubmission(clientId, msg, userId);
                    break;

                case Command.DeleteSubmission:
                    HandleDeleteSubmission(clientId, msg, userId);
                    break;

                case Command.DownloadSubmission:
                    HandleDownloadSubmission(clientId, msg, userId, role);
                    break;

                // Teacher-facing
                case Command.GetClassList:
                    RequireTeacher(clientId, role, () => _server.Reply(clientId, true, data: _class.GetClassesOfTeacher(userId)));
                    break;

                case Command.GetClassStudents:
                    RequireTeacher(clientId, role, () => HandleGetClassStudents(clientId, msg, userId));
                    break;

                case Command.GetTeacherAssignments:
                    RequireTeacher(clientId, role, () => HandleGetTeacherAssignments(clientId, msg, userId));
                    break;

                case Command.CreateAssignment:
                    RequireTeacher(clientId, role, () => HandleCreateAssignment(clientId, msg, userId));
                    break;

                case Command.UpdateAssignment:
                    RequireTeacher(clientId, role, () => HandleUpdateAssignment(clientId, msg, userId));
                    break;

                case Command.DeleteAssignment:
                    RequireTeacher(clientId, role, () => HandleDeleteAssignment(clientId, msg, userId));
                    break;

                case Command.GetClassSubmissions:
                    RequireTeacher(clientId, role, () => HandleGetClassSubmissions(clientId, msg, userId));
                    break;

                case Command.GradeSubmission:
                    RequireTeacher(clientId, role, () => HandleGradeSubmission(clientId, msg, userId));
                    break;

                //case Command.GetStudentList:
                //    RequireTeacher(clientId, role, () => _server.Reply(clientId, true, data: _student.GetAll()));
                //    break;

                case Command.CreateStudent:
                    RequireTeacher(clientId, role, () => HandleCreateStudent(clientId, msg));
                    break;

                case Command.UpdateStudent:
                    RequireTeacher(clientId, role, () => HandleUpdateStudent(clientId, msg));
                    break;

                default:
                    _server.Reply(clientId, false, $"Unknown command: {msg.Type}");
                    break;
            }
        }

        private void RequireTeacher(Guid clientId, Role role, Action action)
        {
            if (role != Role.Teacher)
            {
                _server.Reply(clientId, false, "Chỉ giáo viên mới được thực hiện thao tác này");
                return;
            }
            action();
        }

        // -------------------------------------------------------
        // Auth
        // -------------------------------------------------------

        private void HandleLogin(Guid clientId, Message msg)
        {
            var data = Deserialize<LoginRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var result = _auth.Login(data.UserId, data.Password, data.Role);
            if (result == null)
            {
                _server.Reply(clientId, false, "Sai tài khoản hoặc mật khẩu");
                return;
            }

            _server.Reply(clientId, true, data: result);
        }

        // -------------------------------------------------------
        // Assignment (student)
        // -------------------------------------------------------

        private void HandleGetAssignmentDetail(Guid clientId, Message msg, string studentId)
        {
            var data = Deserialize<GetAssignmentDetailRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var assignment = _assignment.GetAssignmentDetail(data.AssignmentId, studentId);
            if (assignment == null)
            {
                _server.Reply(clientId, false, "Assignment not found or access denied");
                return;
            }

            _server.Reply(clientId, true, data: assignment);
        }

        private void HandleDownloadProblemFile(Guid clientId, Message msg, string userId, Role role)
        {
            var data = Deserialize<DownloadProblemFileRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var path = role == Role.Student
                ? _assignment.GetProblemFilePath(data.AssignmentId, userId)
                : _teacherAssignment.GetProblemFilePath(data.AssignmentId, userId);

            if (path == null)
            {
                _server.Reply(clientId, false, "Problem file not found or access denied");
                return;
            }

            SendFileToClient(clientId, path);
        }

        // -------------------------------------------------------
        // Upload submission (student)
        // -------------------------------------------------------

        private void HandleSubmitAssignment(Guid clientId, Message msg, string studentId)
        {
            var data = Deserialize<SubmitAssignmentRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var uploadId = _submission.BeginUpload(data.AssignmentId, studentId, data.FileName, data.FileSize);
            _server.Reply(clientId, true, data: new SubmitReadyResponse { UploadId = uploadId });
        }

        private void HandleFileChunk(Guid clientId, Message msg)
        {
            var data = Deserialize<FileChunkData>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            try
            {
                _submission.AddChunk(data.UploadId, data.ChunkIndex, data.Base64Data);
                _server.Reply(clientId, true);
            }
            catch (Exception ex)
            {
                _server.Reply(clientId, false, ex.Message);
            }
        }

        private void HandleFileEnd(Guid clientId, Message msg)
        {
            var data = Deserialize<FileEndData>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            try
            {
                var submissionId = _submission.FinalizeUpload(data.UploadId);
                _server.Reply(clientId, true, data: new SubmitOkResponse { SubmissionId = submissionId });
            }
            catch (Exception ex)
            {
                _server.Reply(clientId, false, ex.Message);
            }
        }

        // -------------------------------------------------------
        // Submission management (student)
        // -------------------------------------------------------

        private void HandleGetSubmission(Guid clientId, Message msg, string studentId)
        {
            var data = Deserialize<GetSubmissionRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var submission = _submission.GetSubmissionByAssignment(data.AssignmentId, studentId);
            if (submission == null)
            {
                _server.Reply(clientId, false, "Submission not found or access denied");
                return;
            }

            _server.Reply(clientId, true, data: submission);
        }

        private void HandleDeleteSubmission(Guid clientId, Message msg, string studentId)
        {
            var data = Deserialize<DeleteSubmissionRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var ok = _submission.DeleteSubmission(data.SubmissionId, studentId);
            if (!ok)
            {
                _server.Reply(clientId, false, "Submission not found or access denied");
                return;
            }

            _server.Reply(clientId, true);
        }

        private void HandleDownloadSubmission(Guid clientId, Message msg, string userId, Role role)
        {
            var data = Deserialize<DownloadSubmissionRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var path = _submission.GetSubmissionFilePath(data.SubmissionId, userId, role);
            if (path == null)
            {
                _server.Reply(clientId, false, "File not found or access denied");
                return;
            }

            SendFileToClient(clientId, path);
        }

        // -------------------------------------------------------
        // Class (teacher, chi xem)
        // -------------------------------------------------------

        private void HandleGetClassStudents(Guid clientId, Message msg, string teacherId)
        {
            var data = Deserialize<GetClassStudentsRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var result = _class.GetClassStudents(teacherId, data.ClassId);
            if (result == null)
            {
                _server.Reply(clientId, false, "Lớp không tồn tại hoặc không thuộc quyền quản lý");
                return;
            }

            _server.Reply(clientId, true, data: result);
        }

        // -------------------------------------------------------
        // Assignment CRUD (teacher)
        // -------------------------------------------------------

        private void HandleGetTeacherAssignments(Guid clientId, Message msg, string teacherId)
        {
            var data = Deserialize<GetTeacherAssignmentsRequest>(msg.Data) ?? new GetTeacherAssignmentsRequest();
            _server.Reply(clientId, true, data: _teacherAssignment.GetAll(teacherId, data.ClassId));
        }

        private void HandleCreateAssignment(Guid clientId, Message msg, string teacherId)
        {
            var data = Deserialize<CreateAssignmentRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var result = _teacherAssignment.Create(teacherId, data);
            if (result == null)
            {
                _server.Reply(clientId, false, "Lớp không tồn tại hoặc không thuộc quyền quản lý");
                return;
            }

            _server.Reply(clientId, true, data: result);
        }

        private void HandleUpdateAssignment(Guid clientId, Message msg, string teacherId)
        {
            var data = Deserialize<UpdateAssignmentRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var result = _teacherAssignment.Update(teacherId, data);
            if (result == null)
            {
                _server.Reply(clientId, false, "Bài tập không tồn tại hoặc không thuộc quyền quản lý");
                return;
            }

            _server.Reply(clientId, true, data: result);
        }

        private void HandleDeleteAssignment(Guid clientId, Message msg, string teacherId)
        {
            var data = Deserialize<DeleteAssignmentRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var ok = _teacherAssignment.Delete(teacherId, data.Id);
            _server.Reply(clientId, ok, ok ? "" : "Không tìm thấy bài tập");
        }

        // -------------------------------------------------------
        // Grading (teacher)
        // -------------------------------------------------------

        private void HandleGetClassSubmissions(Guid clientId, Message msg, string teacherId)
        {
            var data = Deserialize<GetClassSubmissionsRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            _server.Reply(clientId, true, data: _submission.GetClassSubmissions(data.AssignmentId, teacherId));
        }

        private void HandleGradeSubmission(Guid clientId, Message msg, string teacherId)
        {
            var data = Deserialize<GradeSubmissionRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var ok = _submission.SetGrade(data.SubmissionId, teacherId, data.Grade);
            _server.Reply(clientId, ok, ok ? "" : "Không tìm thấy bài nộp");
        }

        // -------------------------------------------------------
        // Quan ly tai khoan sinh vien (teacher)
        // -------------------------------------------------------

        private void HandleCreateStudent(Guid clientId, Message msg)
        {
            var data = Deserialize<CreateStudentRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var result = _student.Add(data);
            _server.Reply(clientId, true, data: result);
        }

        private void HandleUpdateStudent(Guid clientId, Message msg)
        {
            var data = Deserialize<UpdateStudentRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var result = _student.Update(data);
            if (result == null)
            {
                _server.Reply(clientId, false, "Không tìm thấy sinh viên");
                return;
            }

            _server.Reply(clientId, true, data: result);
        }

        // -------------------------------------------------------
        // Shared: stream file to client as chunks
        // -------------------------------------------------------

        private void SendFileToClient(Guid clientId, string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var fileBytes = File.ReadAllBytes(filePath);
            var totalChunks = (int)Math.Ceiling((double)fileBytes.Length / ChunkSize);

            for (int i = 0; i < totalChunks; i++)
            {
                var offset = i * ChunkSize;
                var length = Math.Min(ChunkSize, fileBytes.Length - offset);
                var chunk = new byte[length];
                Array.Copy(fileBytes, offset, chunk, 0, length);

                var chunkMsg = new Message
                {
                    Type = Command.FileChunkDown,
                    Data = new FileChunkDownData
                    {
                        ChunkIndex = i,
                        Base64Data = Convert.ToBase64String(chunk)
                    }
                };

                _server.Reply(clientId, true, data: chunkMsg);
            }

            var endMsg = new Message
            {
                Type = Command.FileEndDown,
                Data = new FileEndDownData
                {
                    FileName = fileName,
                    FileSize = fileBytes.Length
                }
            };
            _server.Reply(clientId, true, data: endMsg);
        }

        private static T? Deserialize<T>(object? data) where T : class
        {
            if (data is JsonElement el)
                return el.Deserialize<T>(JsonHandle.JsonOpts);
            return null;
        }
    }
}
