using Microsoft.Extensions.Configuration;
using System.IO;
using QLBT.Server.Handlers;
using QLBT.Server.Models;
using QLBT.Server.Services;
using QLBT.Shared.Transport;

namespace QLBT.Server
{
    /// <summary>
    /// Composition root: quan ly vong doi TcpServer + MessageHandler va cac Service dung chung.
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
        public IClassService ClassService { get; }
        public ITeacherAssignmentService TeacherAssignmentService { get; }
        public IStudentService StudentService { get; }
        public IAdminService AdminService { get; }

        public ServerHost()
        {
            Db = new Prn212PQlbtContext();

            AssignmentService = new AssignmentService(Db);
            var submissionService = new SubmissionService(Db);
            SubmissionService = submissionService;
            ClassService = new ClassService(Db);
            var teacherAssignmentService = new TeacherAssignmentService(Db);
            TeacherAssignmentService = teacherAssignmentService;
            StudentService = new StudentService(Db);
            AdminService = new AdminService(Db);

            var settings = LoadSettings();
            submissionService.RootFolder = settings.RootFolder;
            teacherAssignmentService.RootFolder = settings.RootFolder;
        }

        public ServerSettings LoadSettings()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(SettingsPath, optional: true, reloadOnChange: false)
                .Build();

            return new ServerSettings
            {
                IpAddress = config["ServerSettings:IpAddress"] ?? "0.0.0.0",
                Port = int.TryParse(config["ServerSettings:Port"], out var port) ? port : 9000,
                RootFolder = config["ServerSettings:RootFolder"] ?? "Submissions"
            };
        }

        /// <summary>Ghi de muc ServerSettings trong appsettings.json, giu nguyen ConnectionStrings.</summary>
        public void SaveSettings(ServerSettings settings)
        {
            var path = Path.Combine(AppContext.BaseDirectory, SettingsPath);
            var json = File.Exists(path) ? File.ReadAllText(path) : "{}";
            var node = System.Text.Json.Nodes.JsonNode.Parse(json) ?? new System.Text.Json.Nodes.JsonObject();

            node["ServerSettings"] = new System.Text.Json.Nodes.JsonObject
            {
                ["IpAddress"] = settings.IpAddress,
                ["Port"] = settings.Port,
                ["RootFolder"] = settings.RootFolder
            };

            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, node.ToJsonString(options));

            ((SubmissionService)SubmissionService).RootFolder = settings.RootFolder;
            ((TeacherAssignmentService)TeacherAssignmentService).RootFolder = settings.RootFolder;
        }

        /// <summary>Khoi dong TCP server. Neu dang chay, tu Stop truoc khi khoi dong lai.</summary>
        public void Start(ServerSettings settings)
        {
            if (IsRunning) Stop();

            ((SubmissionService)SubmissionService).RootFolder = settings.RootFolder;

            _tcpServer = new TcpServer(settings.IpAddress, settings.Port);
            _handler = new MessageHandler(_tcpServer, new AuthService(Db), AssignmentService, SubmissionService, ClassService, TeacherAssignmentService);

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
