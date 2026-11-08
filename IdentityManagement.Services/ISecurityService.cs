using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.Services
{
    public interface ISecurityService 
    {
        string GetSha256Hash(string input);
    }
}
