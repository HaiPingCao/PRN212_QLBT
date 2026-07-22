using QLBT.Shared.Client;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Text.Json;

namespace QLBT.Client.Teacher.Services
{
    public class GradingClientService
    {
        private readonly TcpClient _client;
        private readonly AuthClientService _auth;

        public GradingClientService(TcpClient client, AuthClientService auth)
        {
            _client = client;
            _auth = auth;
        }

        public List<SubmissionDto> GetClassSubmissions(int assignmentId)
        {
            var msg = new Message
            {
                Type = Command.GetClassSubmissions,
                Token = _auth.Token ?? "",
                Data = new GetClassSubmissionsRequest { AssignmentId = assignmentId }
            };
            var response = _client.SendAndWait(msg);
            if (response == null || !response.Success) return new List<SubmissionDto>();
            return (response.Data as JsonElement?)?.Deserialize<List<SubmissionDto>>(JsonHandle.JsonOpts) ?? new List<SubmissionDto>();
        }

        public bool SetGrade(int submissionId, decimal? grade)
        {
            var msg = new Message
            {
                Type = Command.GradeSubmission,
                Token = _auth.Token ?? "",
                Data = new GradeSubmissionRequest { SubmissionId = submissionId, Grade = grade }
            };
            var response = _client.SendAndWait(msg);
            return response?.Success ?? false;
        }

        /// <summary>Tai bai nop cua sinh vien (quyen giao vien). Tra ve duong dan da luu, null neu that bai.</summary>
        public string? DownloadSubmission(int submissionId, string saveFolder)
        {
            var msg = new Message
            {
                Type = Command.DownloadSubmission,
                Token = _auth.Token ?? "",
                Data = new DownloadSubmissionRequest { SubmissionId = submissionId }
            };
            return FileReceiver.Receive(_client, msg, saveFolder);
        }
    }
}
