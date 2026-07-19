using QLBT.Shared.Models;

namespace QLBT.Server.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates the student. Returns token on success, null on failure.
        /// </summary>
        LoginResponse? Login(string studentId, string password);

        /// <summary>
        /// Validates token. Returns studentId if valid, null if not.
        /// </summary>
        string? ValidateToken(string token);

        void Logout(string token);
    }
}