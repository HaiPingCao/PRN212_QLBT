using QLBT.Server.Models;
using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly Prn212PQlbtContext _db;

        // token → studentId, in-memory, resets on server restart
        private readonly Dictionary<string, string> _sessions = new();

        public AuthService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public LoginResponse? Login(string studentId, string password)
        {
            var sv = _db.SinhViens.FirstOrDefault(s => s.Mssv == studentId);
            if (sv == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(password, sv.MatKhau)) return null;

            var token = Guid.NewGuid().ToString();
            _sessions[token] = studentId;

            return new LoginResponse
            {
                Token = token,
                FullName = sv.HoTen
            };
        }

        public string? ValidateToken(string token)
        {
            _sessions.TryGetValue(token, out var studentId);
            return studentId;
        }

        public void Logout(string token)
        {
            _sessions.Remove(token);
        }
    }
}