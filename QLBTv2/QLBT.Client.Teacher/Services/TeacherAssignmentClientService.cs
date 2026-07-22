using QLBT.Shared.Client;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.IO;
using System.Text.Json;

namespace QLBT.Client.Teacher.Services
{
    public class TeacherAssignmentClientService
    {
        private readonly TcpClient _client;
        private readonly AuthClientService _auth;

        public TeacherAssignmentClientService(TcpClient client, AuthClientService auth)
        {
            _client = client;
            _auth = auth;
        }

        public List<TeacherAssignmentDto> GetAll(int classId = 0)
        {
            var msg = new Message
            {
                Type = Command.GetTeacherAssignments,
                Token = _auth.Token ?? "",
                Data = new GetTeacherAssignmentsRequest { ClassId = classId }
            };
            var response = _client.SendAndWait(msg);
            if (response == null || !response.Success) return new List<TeacherAssignmentDto>();
            return (response.Data as JsonElement?)?.Deserialize<List<TeacherAssignmentDto>>(JsonHandle.JsonOpts) ?? new List<TeacherAssignmentDto>();
        }

        public TeacherAssignmentDto? Create(int classId, string title, string? description, DateTime dueDate, string? problemFilePath)
        {
            var (fileName, base64) = ReadFileAsBase64(problemFilePath);

            var msg = new Message
            {
                Type = Command.CreateAssignment,
                Token = _auth.Token ?? "",
                Data = new CreateAssignmentRequest
                {
                    ClassId = classId,
                    Title = title,
                    Description = description,
                    DueDate = dueDate,
                    ProblemFileName = fileName,
                    ProblemFileBase64 = base64
                }
            };

            var response = _client.SendAndWait(msg, timeoutMs: 30000);
            if (response == null || !response.Success) return null;
            return (response.Data as JsonElement?)?.Deserialize<TeacherAssignmentDto>(JsonHandle.JsonOpts);
        }

        public TeacherAssignmentDto? Update(int id, string title, string? description, DateTime dueDate, string? problemFilePath)
        {
            var (fileName, base64) = ReadFileAsBase64(problemFilePath);

            var msg = new Message
            {
                Type = Command.UpdateAssignment,
                Token = _auth.Token ?? "",
                Data = new UpdateAssignmentRequest
                {
                    Id = id,
                    Title = title,
                    Description = description,
                    DueDate = dueDate,
                    ProblemFileName = fileName,
                    ProblemFileBase64 = base64
                }
            };

            var response = _client.SendAndWait(msg, timeoutMs: 30000);
            if (response == null || !response.Success) return null;
            return (response.Data as JsonElement?)?.Deserialize<TeacherAssignmentDto>(JsonHandle.JsonOpts);
        }

        public bool Delete(int id)
        {
            var msg = new Message
            {
                Type = Command.DeleteAssignment,
                Token = _auth.Token ?? "",
                Data = new DeleteAssignmentRequest { Id = id }
            };
            var response = _client.SendAndWait(msg);
            return response?.Success ?? false;
        }

        /// <summary>Tai file de cua bai tap thuoc quyen quan ly cua giao vien nay. Tra ve duong dan da luu, null neu that bai.</summary>
        public string? DownloadProblemFile(int assignmentId, string saveFolder)
        {
            var msg = new Message
            {
                Type = Command.DownloadProblemFile,
                Token = _auth.Token ?? "",
                Data = new DownloadProblemFileRequest { AssignmentId = assignmentId }
            };
            return FileReceiver.Receive(_client, msg, saveFolder);
        }

        private static (string? FileName, string? Base64) ReadFileAsBase64(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return (null, null);

            var bytes = File.ReadAllBytes(filePath);
            return (Path.GetFileName(filePath), Convert.ToBase64String(bytes));
        }
    }
}
