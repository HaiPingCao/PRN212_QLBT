using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Text.Json;

namespace QLBT.Client.Services
{
    public class SubmissionClientService
    {
        private readonly TcpClient _client;
        private readonly AuthClientService _auth;

        private const int ChunkSize = 64 * 1024; // 64KB — must match server

        public SubmissionClientService(TcpClient client, AuthClientService auth)
        {
            _client = client;
            _auth = auth;
        }

        /// <summary>
        /// Uploads the submission file. Returns submissionId on success, -1 on failure.
        /// </summary>
        public int SubmitAssignment(int assignmentId, string filePath)
        {
            if (!File.Exists(filePath)) return -1;

            var fileName = Path.GetFileName(filePath);
            var fileBytes = File.ReadAllBytes(filePath);
            var fileSize = fileBytes.Length;

            // Step 1: send metadata, receive uploadId
            var initMsg = new Message
            {
                Type = Command.SUBMIT_ASSIGNMENT,
                Token = _auth.Token ?? "",
                Data = new SubmitAssignmentRequest
                {
                    AssignmentId = assignmentId,
                    FileName = fileName,
                    FileSize = fileSize
                }
            };

            var initResponse = _client.SendAndWait(initMsg);
            if (initResponse == null || !initResponse.Success) return -1;

            var ready = (initResponse.Data as JsonElement?)
                ?.Deserialize<SubmitReadyResponse>(JsonHandle.JsonOpts);
            if (ready == null) return -1;

            var uploadId = ready.UploadId;

            // Step 2: send chunks
            var totalChunks = (int)Math.Ceiling((double)fileSize / ChunkSize);

            for (int i = 0; i < totalChunks; i++)
            {
                var offset = i * ChunkSize;
                var length = Math.Min(ChunkSize, fileSize - offset);
                var chunk = new byte[length];
                Array.Copy(fileBytes, offset, chunk, 0, length);

                var chunkMsg = new Message
                {
                    Type = Command.FILE_CHUNK,
                    Token = _auth.Token ?? "",
                    Data = new FileChunkData
                    {
                        UploadId = uploadId,
                        ChunkIndex = i,
                        Base64Data = Convert.ToBase64String(chunk)
                    }
                };

                var chunkResponse = _client.SendAndWait(chunkMsg);
                if (chunkResponse == null || !chunkResponse.Success) return -1;
            }

            // Step 3: signal end
            var endMsg = new Message
            {
                Type = Command.FILE_END,
                Token = _auth.Token ?? "",
                Data = new FileEndData { UploadId = uploadId }
            };

            var endResponse = _client.SendAndWait(endMsg);
            if (endResponse == null || !endResponse.Success) return -1;

            var ok = (endResponse.Data as JsonElement?)
                ?.Deserialize<SubmitOkResponse>(JsonHandle.JsonOpts);

            return ok?.SubmissionId ?? -1;
        }

        public SubmissionDto? GetSubmission(int submissionId)
        {
            var msg = new Message
            {
                Type = Command.GET_SUBMISSION,
                Token = _auth.Token ?? "",
                Data = new GetSubmissionRequest { SubmissionId = submissionId }
            };

            var response = _client.SendAndWait(msg);
            if (response == null || !response.Success) return null;

            return (response.Data as JsonElement?)
                ?.Deserialize<SubmissionDto>(JsonHandle.JsonOpts);
        }

        public bool DeleteSubmission(int submissionId)
        {
            var msg = new Message
            {
                Type = Command.DELETE_SUBMISSION,
                Token = _auth.Token ?? "",
                Data = new DeleteSubmissionRequest { SubmissionId = submissionId }
            };

            var response = _client.SendAndWait(msg);
            return response?.Success ?? false;
        }

        /// <summary>
        /// Downloads the student's own submission file.
        /// Returns the saved file path, null on failure.
        /// </summary>
        public string? DownloadSubmission(int submissionId, string saveFolder)
        {
            var msg = new Message
            {
                Type = Command.DOWNLOAD_SUBMISSION,
                Token = _auth.Token ?? "",
                Data = new DownloadSubmissionRequest { SubmissionId = submissionId }
            };
            return FileReceiver.Receive(_client, msg, saveFolder);
        }
    }
}