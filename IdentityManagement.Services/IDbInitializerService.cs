using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.Services
{
    public interface IDbInitializerService
    {
        void Initialize();
        void SeedData();
    }
}
