using QLBT.Server.Services;
using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Text.Json;

namespace QLBT.Server.Handlers
{
    public class MessageHandler
    {
        private readonly TcpServer _server;
        private readonly IAuthService _auth;
        private readonly IAssignmentService _assignment;
        private readonly ISubmissionService _submission;

        private const int ChunkSize = 64 * 1024; // 64KB per chunk

        public MessageHandler(
            TcpServer server,
            IAuthService auth,
            IAssignmentService assignment,
            ISubmissionService submission)
        {
            _server = server;
            _auth = auth;
            _assignment = assignment;
            _submission = submission;

            _server.OnMessage = Handle;
        }

        private void Handle(Guid clientId, Message msg)
        {
            // LOGIN does not require a token
            if (msg.Type == Command.LOGIN)
            {
                HandleLogin(clientId, msg);
                return;
            }

            // All other commands require a valid token
            var studentId = _auth.ValidateToken(msg.Token);
            if (studentId == null)
            {
                _server.Reply(clientId, false, "Invalid or expired token");
                return;
            }

            switch (msg.Type)
            {
                case Command.GET_ASSIGNMENT_LIST:
                    HandleGetAssignmentList(clientId, msg, studentId);
                    break;

                case Command.GET_ASSIGNMENT_DETAIL:
                    HandleGetAssignmentDetail(clientId, msg, studentId);
                    break;

                case Command.DOWNLOAD_PROBLEM_FILE:
                    HandleDownloadProblemFile(clientId, msg, studentId);
                    break;

                case Command.SUBMIT_ASSIGNMENT:
                    HandleSubmitAssignment(clientId, msg, studentId);
                    break;

                case Command.FILE_CHUNK:
                    HandleFileChunk(clientId, msg, studentId);
                    break;

                case Command.FILE_END:
                    HandleFileEnd(clientId, msg, studentId);
                    break;

                case Command.GET_SUBMISSION:
                    HandleGetSubmission(clientId, msg, studentId);
                    break;

                case Command.DELETE_SUBMISSION:
                    HandleDeleteSubmission(clientId, msg, studentId);
                    break;

                case Command.DOWNLOAD_SUBMISSION:
                    HandleDownloadSubmission(clientId, msg, studentId);
                    break;

                default:
                    _server.Reply(clientId, false, $"Unknown command: {msg.Type}");
                    break;
            }
        }

        // -------------------------------------------------------
        // Auth
        // -------------------------------------------------------

        private void HandleLogin(Guid clientId, Message msg)
        {
            var data = Deserialize<LoginRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var result = _auth.Login(data.StudentId, data.Password);
            if (result == null)
            {
                _server.Reply(clientId, false, "Invalid username or password");
                return;
            }

            _server.Reply(clientId, true, data: result);
        }

        // -------------------------------------------------------
        // Assignment
        // -------------------------------------------------------

        private void HandleGetAssignmentList(Guid clientId, Message msg, string studentId)
        {
            var list = _assignment.GetAssignmentList(studentId);
            _server.Reply(clientId, true, data: list);
        }

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

        // -------------------------------------------------------
        // Download problem file
        // -------------------------------------------------------

        private void HandleDownloadProblemFile(Guid clientId, Message msg, string studentId)
        {
            var data = Deserialize<DownloadProblemFileRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var path = _submission.GetProblemFilePath(data.AssignmentId, studentId);
            if (path == null)
            {
                _server.Reply(clientId, false, "Problem file not found or access denied");
                return;
            }

            SendFileToClient(clientId, path);
        }

        // -------------------------------------------------------
        // Upload submission
        // -------------------------------------------------------

        private void HandleSubmitAssignment(Guid clientId, Message msg, string studentId)
        {
            var data = Deserialize<SubmitAssignmentRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var uploadId = _submission.BeginUpload(
                data.AssignmentId, studentId, data.FileName, data.FileSize);

            _server.Reply(clientId, true, data: new SubmitReadyResponse { UploadId = uploadId });
        }

        private void HandleFileChunk(Guid clientId, Message msg, string studentId)
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

        private void HandleFileEnd(Guid clientId, Message msg, string studentId)
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
        // Submission management
        // -------------------------------------------------------

        private void HandleGetSubmission(Guid clientId, Message msg, string studentId)
        {
            var data = Deserialize<GetSubmissionRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var submission = _submission.GetSubmission(data.SubmissionId, studentId);
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

        // -------------------------------------------------------
        // Download submission
        // -------------------------------------------------------

        private void HandleDownloadSubmission(Guid clientId, Message msg, string studentId)
        {
            var data = Deserialize<DownloadSubmissionRequest>(msg.Data);
            if (data == null) { _server.Reply(clientId, false, "Data null"); return; }

            var path = _submission.GetSubmissionFilePath(data.SubmissionId, studentId);
            if (path == null)
            {
                _server.Reply(clientId, false, "File not found or access denied");
                return;
            }

            SendFileToClient(clientId, path);
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
                    Type = Command.FILE_CHUNK_DOWN,
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
                Type = Command.FILE_END_DOWN,
                Data = new FileEndDownData
                {
                    FileName = fileName,
                    FileSize = fileBytes.Length
                }
            };
            _server.Reply(clientId, true, data: endMsg);
        }

        // -------------------------------------------------------
        // Helper
        // -------------------------------------------------------

        private static T? Deserialize<T>(object? data) where T : class
        {
            if (data is JsonElement el)
                return el.Deserialize<T>(JsonHandle.JsonOpts);
            return null;
        }
    }
}