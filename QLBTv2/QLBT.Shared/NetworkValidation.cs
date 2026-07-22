using System.Net;

namespace QLBT.Shared
{
    /// <summary>
    /// Kiem tra hop le dia chi IP / cong, dung chung cho man hinh cai dat Server
    /// va man hinh ket noi cua ca hai Client (giao vien/sinh vien).
    /// </summary>
    public static class NetworkValidation
    {
        public static bool IsValidIp(string? ip)
        {
            return !string.IsNullOrWhiteSpace(ip) && IPAddress.TryParse(ip.Trim(), out _);
        }

        public static bool IsValidPort(string? port, out int parsed)
        {
            return int.TryParse(port, out parsed) && parsed > 0 && parsed <= 65535;
        }
    }
}
