using IdentityManagement.DomainClasses;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.Services
{
    public interface IRolesService
    {
        Task<List<Role>> FindUserRolesAsync(int userId);
        Task<bool> IsUserInRole(int userId, string roleName);
        Task<List<User>> FindUsersInRoleAsync(string roleName);
    }
}
