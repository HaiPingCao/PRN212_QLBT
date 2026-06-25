using QLBT.Shared.Models;
using System.Text;
using System.Text.Json;
using WatsonTcp;

namespace QLBT.Shared.Transport
{
    public sealed class TcpServer : IDisposable
    {
        private readonly WatsonTcpServer _server;

        public bool IsRunning { get; private set; }

        /// <summary>
        /// MessageHandler tại QLBT.Server đăng ký vào delegate này.
        /// TcpServer không biết gì về business logic.
        /// </summary>
        public Action<Guid, Message>? OnMessage { get; set; }

        public TcpServer(string ipAddress, int port)
        {
            _server = new WatsonTcpServer(ipAddress, port);

            _server.Events.ClientConnected += (_, args) =>
                Console.WriteLine($"[+] Client connected: {args.Client.IpPort}");

            _server.Events.ClientDisconnected += (_, args) =>
                Console.WriteLine($"[-] Client disconnected: {args.Client.IpPort}");

            _server.Events.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            string raw = Encoding.UTF8.GetString(args.Data);
            Guid clientId = args.Client.Guid;

            Message? msg;
            try
            {
                msg = JsonSerializer.Deserialize<Message>(raw, JsonHandle.JsonOpts);
            }
            catch
            {
                Reply(clientId, false, "JSON khong hop le");
                return;
            }

            if (msg == null)
            {
                Reply(clientId, false, "Goi tin null");
                return;
            }

            if (OnMessage == null)
            {
                Reply(clientId, false, "Server chua san sang");
                return;
            }

            OnMessage(clientId, msg);
        }

        public void Reply(Guid clientId, bool success, string error = "", object? data = null)
        {
            var response = new Response { Success = success, Error = error, Data = data };
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response, JsonHandle.JsonOpts));
            _server.Send(clientId, bytes);
        }

        public void Start()
        {
            if (IsRunning) return;
            _server.Start();
            IsRunning = true;
            Console.WriteLine("Server started");
        }

        public void Stop()
        {
            if (!IsRunning) return;
            _server.Stop();
            IsRunning = false;
            Console.WriteLine("Server stopped");
        }

        public void Dispose()
        {
            Stop();
            _server.Dispose();
        }
    }
}