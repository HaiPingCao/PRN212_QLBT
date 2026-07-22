using QLBT.Shared.Client;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.IO;
using System.Text.Json;

namespace QLBT.Client.Student.Services
{
    public class SubmissionClientService
    {
        private readonly TcpClient _client;
        private readonly AuthClientService _auth;

        private const int ChunkSize = 64 * 1024; // phai khop voi server

        public SubmissionClientService(TcpClient client, AuthClientService auth)
        {
            _client = client;
            _auth = auth;
        }

        /// <summary>Tai len file bai lam. Tra ve submissionId neu thanh cong, -1 neu that bai.</summary>
        public int SubmitAssignment(int assignmentId, string filePath)
        {
            if (!File.Exists(filePath)) return -1;

            var fileName = Path.GetFileName(filePath);
            var fileBytes = File.ReadAllBytes(filePath);
            var fileSize = fileBytes.Length;

            var initMsg = new Message
            {
                Type = Command.SubmitAssignment,
                Token = _auth.Token ?? "",
                Data = new SubmitAssignmentRequest { AssignmentId = assignmentId, FileName = fileName, FileSize = fileSize }
            };

            var initResponse = _client.SendAndWait(initMsg);
            if (initResponse == null || !initResponse.Success) return -1;

            var ready = (initResponse.Data as JsonElement?)?.Deserialize<SubmitReadyResponse>(JsonHandle.JsonOpts);
            if (ready == null) return -1;

            var uploadId = ready.UploadId;
            var totalChunks = (int)Math.Ceiling((double)fileSize / ChunkSize);

            for (int i = 0; i < totalChunks; i++)
            {
                var offset = i * ChunkSize;
                var length = Math.Min(ChunkSize, fileSize - offset);
                var chunk = new byte[length];
                Array.Copy(fileBytes, offset, chunk, 0, length);

                var chunkMsg = new Message
                {
                    Type = Command.FileChunk,
                    Token = _auth.Token ?? "",
                    Data = new FileChunkData { UploadId = uploadId, ChunkIndex = i, Base64Data = Convert.ToBase64String(chunk) }
                };

                var chunkResponse = _client.SendAndWait(chunkMsg);
                if (chunkResponse == null || !chunkResponse.Success) return -1;
            }

            var endMsg = new Message
            {
                Type = Command.FileEnd,
                Token = _auth.Token ?? "",
                Data = new FileEndData { UploadId = uploadId }
            };

            var endResponse = _client.SendAndWait(endMsg);
            if (endResponse == null || !endResponse.Success) return -1;

            var ok = (endResponse.Data as JsonElement?)?.Deserialize<SubmitOkResponse>(JsonHandle.JsonOpts);
            return ok?.SubmissionId ?? -1;
        }

        public SubmissionDto? GetSubmissionByAssignment(int assignmentId)
        {
            var msg = new Message
            {
                Type = Command.GetSubmission,
                Token = _auth.Token ?? "",
                Data = new GetSubmissionRequest { AssignmentId = assignmentId }
            };

            var response = _client.SendAndWait(msg);
            if (response == null || !response.Success) return null;

            return (response.Data as JsonElement?)?.Deserialize<SubmissionDto>(JsonHandle.JsonOpts);
        }

        public bool DeleteSubmission(int submissionId)
        {
            var msg = new Message
            {
                Type = Command.DeleteSubmission,
                Token = _auth.Token ?? "",
                Data = new DeleteSubmissionRequest { SubmissionId = submissionId }
            };

            var response = _client.SendAndWait(msg);
            return response?.Success ?? false;
        }

        /// <summary>Tai bai nop cua chinh sinh vien. Tra ve duong dan file da luu, null neu that bai.</summary>
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
