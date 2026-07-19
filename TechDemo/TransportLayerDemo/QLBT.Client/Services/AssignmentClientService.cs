using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Text.Json;

namespace QLBT.Client.Services
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
            var msg = new Message
            {
                Type = Command.GET_ASSIGNMENT_LIST,
                Token = _auth.Token ?? ""
            };

            var response = _client.SendAndWait(msg);
            if (response == null || !response.Success) return null;

            return (response.Data as JsonElement?)
                ?.Deserialize<List<AssignmentDto>>(JsonHandle.JsonOpts);
        }

        public AssignmentDto? GetAssignmentDetail(int assignmentId)
        {
            var msg = new Message
            {
                Type = Command.GET_ASSIGNMENT_DETAIL,
                Token = _auth.Token ?? "",
                Data = new GetAssignmentDetailRequest { AssignmentId = assignmentId }
            };

            var response = _client.SendAndWait(msg);
            if (response == null || !response.Success) return null;

            return (response.Data as JsonElement?)
                ?.Deserialize<AssignmentDto>(JsonHandle.JsonOpts);
        }

        /// <summary>
        /// Downloads the problem file to the student's machine.
        /// Returns the saved file path, null on failure.
        /// </summary>
        public string? DownloadProblemFile(int assignmentId, string saveFolder)
        {
            var msg = new Message
            {
                Type = Command.DOWNLOAD_PROBLEM_FILE,
                Token = _auth.Token ?? "",
                Data = new DownloadProblemFileRequest { AssignmentId = assignmentId }
            };
            return FileReceiver.Receive(_client, msg, saveFolder);
        }
    }
}