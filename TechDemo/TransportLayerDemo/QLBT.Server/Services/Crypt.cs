using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLBT.Server.Services;

using BCrypt;


internal class Crypt
{
    static string Encrypt(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    static bool IsValid(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }


}
