using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public interface IAuthService
    {
        /// <summary>Xac thuc nguoi dung theo bang tuong ung voi role duoc yeu cau. Tra ve null neu that bai.</summary>
        LoginResponse? Login(string userId, string password, Role role);

        /// <summary>Kiem tra token. Tra ve (userId, role) neu hop le, null neu khong.</summary>
        (string UserId, Role Role)? ValidateToken(string token);

        void Logout(string token);
    }
}
