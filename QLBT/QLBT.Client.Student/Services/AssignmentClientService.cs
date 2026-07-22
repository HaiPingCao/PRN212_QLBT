using QLBT.Shared.Client;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Text.Json;

namespace QLBT.Client.Student.Services
{
    public class AssignmentClientService
    {
        private readonly TcpClient _client;
        private readonly AuthClientService _auth;

        public AssignmentClientService(TcpClient client, AuthClientService auth)
        {
            _client = client;
            _auth = auth;
        }

        public List<AssignmentDto>? GetAssignmentList()
        {
            var msg = new Message { Type = Command.GetAssignmentList, Token = _auth.Token ?? "" };

            var response = _client.SendAndWait(msg);
            if (response == null || !response.Success) return null;

            return (response.Data as JsonElement?)?.Deserialize<List<AssignmentDto>>(JsonHandle.JsonOpts);
        }

        public AssignmentDto? GetAssignmentDetail(int assignmentId)
        {
            var msg = new Message
            {
                Type = Command.GetAssignmentDetail,
                Token = _auth.Token ?? "",
                Data = new GetAssignmentDetailRequest { AssignmentId = assignmentId }
            };

            var response = _client.SendAndWait(msg);
            if (response == null || !response.Success) return null;

            return (response.Data as JsonElement?)?.Deserialize<AssignmentDto>(JsonHandle.JsonOpts);
        }

        /// <summary>Tai file de. Tra ve duong dan file da luu, null neu that bai.</summary>
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
    }
}
