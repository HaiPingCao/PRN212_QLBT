using Microsoft.Extensions.Configuration;
using QLBT.Server.Handlers;
using QLBT.Server.Models;
using QLBT.Server.Services;
using QLBT.Shared.Transport;
using System;
using System.IO;

namespace QLBT.Server
{
    /// <summary>
    /// Composition root: quản lý vòng đời TcpServer + MessageHandler, và giữ các Service
    /// dùng chung (DbContext, AssignmentService, SubmissionService, TeacherAssignmentService)
    /// để các View (ServerManagement, AssignmentView) dùng chung mà không tự khởi tạo riêng.
    /// </summary>
    public sealed class ServerHost
    {
        private const string SettingsPath = "appsettings.json";

        private TcpServer? _tcpServer;
        private MessageHandler? _handler;

        public bool IsRunning => _tcpServer?.IsRunning ?? false;

        public Prn212PQlbtContext Db { get; }
        public IAssignmentService AssignmentService { get; }
        public ISubmissionService SubmissionService { get; }
        public ITeacherAssignmentService TeacherAssignmentService { get; }

        public ServerHost()
        {
            Db = new Prn212PQlbtContext();

            var assignmentService = new AssignmentService(Db);
            AssignmentService = assignmentService;

            var submissionService = new SubmissionService(Db, assignmentService);
            SubmissionService = submissionService;

            var teacherAssignmentService = new TeacherAssignmentService(Db);
            TeacherAssignmentService = teacherAssignmentService;

            // Đồng bộ RootFolder ban đầu từ cấu hình đã lưu.
            var settings = LoadSettings();
            submissionService.RootFolder = settings.RootFolder;
            teacherAssignmentService.RootFolder = settings.RootFolder;
        }

        public ServerSettings LoadSettings()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(SettingsPath, optional: true, reloadOnChange: false)
                .Build();

            return new ServerSettings
            {
                IpAddress = config["ServerSettings:IpAddress"] ?? "0.0.0.0",
                Port = int.TryParse(config["ServerSettings:Port"], out var port) ? port : 9000,
                RootFolder = config["ServerSettings:RootFolder"] ?? "Submissions"
            };
        }

        /// <summary>
        /// Ghi đè mục ServerSettings trong appsettings.json, giữ nguyên ConnectionStrings.
        /// </summary>
        public void SaveSettings(ServerSettings settings)
        {
            var json = File.Exists(SettingsPath) ? File.ReadAllText(SettingsPath) : "{}";
            var node = System.Text.Json.Nodes.JsonNode.Parse(json) ?? new System.Text.Json.Nodes.JsonObject();

            node["ServerSettings"] = new System.Text.Json.Nodes.JsonObject
            {
                ["IpAddress"] = settings.IpAddress,
                ["Port"] = settings.Port,
                ["RootFolder"] = settings.RootFolder
            };

            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(SettingsPath, node.ToJsonString(options));

            // RootFolder áp dụng ngay cho các thao tác CRUD bài tập / nộp bài,
            // kể cả khi TCP server chưa Start.
            ((SubmissionService)SubmissionService).RootFolder = settings.RootFolder;
            ((TeacherAssignmentService)TeacherAssignmentService).RootFolder = settings.RootFolder;
        }

        /// <summary>
        /// Khởi động TCP server với cấu hình truyền vào. Nếu server đang chạy,
        /// tự động Stop server cũ trước khi khởi động với cấu hình mới.
        /// </summary>
        public void Start(ServerSettings settings)
        {
            if (IsRunning)
                Stop();

            ((SubmissionService)SubmissionService).RootFolder = settings.RootFolder;

            _tcpServer = new TcpServer(settings.IpAddress, settings.Port);
            _handler = new MessageHandler(_tcpServer, new AuthService(Db), AssignmentService, SubmissionService);

            _tcpServer.Start();
        }

        public void Stop()
        {
            if (_tcpServer == null) return;

            _tcpServer.Stop();
            _tcpServer.Dispose();
            _tcpServer = null;
            _handler = null;
        }
    }

    public class ServerSettings
    {
        public string IpAddress { get; set; } = "0.0.0.0";
        public int Port { get; set; } = 9000;
        public string RootFolder { get; set; } = "Submissions";
    }
}
