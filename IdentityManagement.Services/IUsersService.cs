using IdentityManagement.DomainClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.Services
{
    public interface IUsersService
    {
        Task<string?> GetSerialNumberAsync(int userId);
        Task<User?> CheckCredsAsync(string username, string password);
        ValueTask<User?> FindUserAsync(int userId);
        Task UpdateUserLastActivityDateAsync(int userId);
    }
}
