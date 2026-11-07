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

        //============================================


        Task<List<User>> GetAllUsersAsync();
        Task<bool> CreateUserAsync(User user);

        Task<User?> GetUserForEditAsync(int userId);

        Task<bool> UpdateUserAsync(User user);

        Task<bool> DeleteUserAsync(int userId);
    }
}
