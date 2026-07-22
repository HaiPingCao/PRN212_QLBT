using QLBT.Shared.Models;
using QLBT.Shared.Transport;
using System.Text.Json;

namespace QLBT.Shared.Client
{
    /// <summary>Dung chung cho Teacher/Student client — chi khac nhau o Role truyen vao Login.</summary>
    public class AuthClientService
    {
        private readonly TcpClient _client;

        public string? Token { get; private set; }
        public string? FullName { get; private set; }
        public bool IsLoggedIn => Token != null;

        public AuthClientService(TcpClient client)
        {
            _client = client;
        }

        public bool Login(string userId, string password, Role role)
        {
            var msg = new Message
            {
                Type = Command.Login,
                Data = new LoginRequest { UserId = userId, Password = password, Role = role }
            };

            var response = _client.SendAndWait(msg);
            if (response == null || !response.Success) return false;

            var data = (response.Data as JsonElement?)?.Deserialize<LoginResponse>(JsonHandle.JsonOpts);
            if (data == null) return false;

            Token = data.Token;
            FullName = data.FullName;
            return true;
        }

        public void Logout()
        {
            Token = null;
            FullName = null;
        }
    }
}
