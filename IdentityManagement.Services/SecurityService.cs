using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace IdentityManagement.Services
{
    public class SecurityService : ISecurityService
    {
        

        public string GetSha256Hash(string input)
        {
            var byteValue = Encoding.UTF8.GetBytes(input);
            var byteHash = SHA256.HashData(byteValue);

            return Convert.ToBase64String(byteHash);
        }
    }
}
