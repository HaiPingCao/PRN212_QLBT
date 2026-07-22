using QLBT.Shared.Models;
using System.Text;
using System.Text.Json;
using WatsonTcp;

namespace QLBT.Shared.Transport
{
    public sealed class TcpClient : IDisposable
    {
        private readonly WatsonTcpClient _client;
        private readonly ManualResetEventSlim _responseReady = new(false);
        private readonly Queue<Response> _responseQueue = new();
        private readonly object _queueLock = new();

        public bool IsConnected => _client.Connected;

        public TcpClient(string host, int port)
        {
            _client = new WatsonTcpClient(host, port);

            _client.Events.ServerConnected += (_, args) =>
            {
                Console.WriteLine($"[+] Connected to server!");
            };

            _client.Events.ServerDisconnected += (_, args) =>
            {
                Console.WriteLine($"[-] Disconnected from server!");
            };

            _client.Events.MessageReceived += OnMessageReceived;
        }
        private void OnMessageReceived(object? sender, MessageReceivedEventArgs args)
        {
            string raw = Encoding.UTF8.GetString(args.Data);
            Response? resp;
            try
            {
                resp = JsonSerializer.Deserialize<Response>(raw, JsonHandle.JsonOpts);
            }
            catch { resp = null; }

            if (resp == null) return;

            lock (_queueLock)
            {
                _responseQueue.Enqueue(resp);
            }
            _responseReady.Set();
        }

        public Response? SendAndWait(Message msg, int timeoutMs = 5000)
        {
            lock (_queueLock) _responseQueue.Clear();
            _responseReady.Reset();
            SendMessage(msg);
            return WaitForNext(timeoutMs);
        }

        public Response? WaitForNext(int timeoutMs = 5000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

            while (DateTime.UtcNow < deadline)
            {
                lock (_queueLock)
                {
                    if (_responseQueue.Count > 0)
                        return _responseQueue.Dequeue();
                }
                _responseReady.Wait(50);
                _responseReady.Reset();
            }

            return null;
        }

        public void Connect()
        {
            if (!IsConnected)
            {
                _client.Connect();
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                _client.Disconnect();
            }
        }

        public bool SendMessage(Message msg)
        {
            if (!IsConnected) return false;
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg, JsonHandle.JsonOpts));
            return _client.Send(bytes);
        }

        public void Dispose()
        {
            Disconnect();
            _client.Dispose();
        }
    }
}
