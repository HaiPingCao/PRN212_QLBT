using System.Text.RegularExpressions;

namespace QLBT.Shared
{
    /// <summary>
    /// Kiem tra hop le du lieu nhap tren cac form (email, do dai chuoi khop cot CSDL,
    /// mat khau toi thieu). Dung chung cho ca 3 ung dung de tranh loi SQL "chung chung"
    /// khi gui du lieu vuot qua rang buoc cot VARCHAR/NVARCHAR.
    /// </summary>
    public static class InputValidation
    {
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // MSSV: dung 5 ky tu, gom chu va so.
        private static readonly Regex MssvRegex = new(
            @"^[A-Za-z0-9]{5}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        // Ma lop: dung 6 chu so.
        private static readonly Regex ClassCodeRegex = new(
            @"^[0-9]{6}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public const int MinPasswordLength = 6;
        public const int MaxHocKyNameLength = 6;

        public static bool IsValidEmail(string? email)
        {
            return !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());
        }

        public static bool IsValidMssv(string? mssv)
        {
            return mssv != null && MssvRegex.IsMatch(mssv.Trim());
        }

        public static bool IsValidClassCode(string? tenLop)
        {
            return tenLop != null && ClassCodeRegex.IsMatch(tenLop.Trim());
        }

        /// <summary>Kiem tra chuoi khong rong va khong vuot qua do dai cot CSDL tuong ung.</summary>
        public static bool IsWithinLength(string? value, int maxLength)
        {
            return value != null && value.Trim().Length <= maxLength;
        }

        public static bool IsValidPassword(string? password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= MinPasswordLength;
        }
    }
}
