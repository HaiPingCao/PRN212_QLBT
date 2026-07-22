using QLBT.Server.Models;
using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly Prn212PQlbtContext _db;

        // token -> (userId, role), luu trong bo nho, mat khi server restart
        private readonly Dictionary<string, (string UserId, Role Role)> _sessions = new();
        private readonly object _lock = new();

        public AuthService(Prn212PQlbtContext db)
        {
            _db = db;
        }

        public LoginResponse? Login(string userId, string password, Role role)
        {
            string hoTen;

            if (role == Role.Student)
            {
                var sv = _db.SinhViens.FirstOrDefault(s => s.Mssv == userId);
                if (sv == null || !BCrypt.Net.BCrypt.Verify(password, sv.MatKhau)) return null;
                hoTen = sv.HoTen;
            }
            else
            {
                var gv = _db.GiaoViens.FirstOrDefault(g => g.Msgv == userId);
                if (gv == null || !BCrypt.Net.BCrypt.Verify(password, gv.MatKhau)) return null;
                hoTen = gv.HoTen;
            }

            var token = Guid.NewGuid().ToString();
            lock (_lock) _sessions[token] = (userId, role);

            return new LoginResponse { Token = token, FullName = hoTen, Role = role };
        }

        public (string UserId, Role Role)? ValidateToken(string token)
        {
            lock (_lock)
            {
                return _sessions.TryGetValue(token, out var session) ? session : null;
            }
        }

        public void Logout(string token)
        {
            lock (_lock) _sessions.Remove(token);
        }
    }
}
