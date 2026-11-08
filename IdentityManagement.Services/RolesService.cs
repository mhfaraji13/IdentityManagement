using IdentityManagement.DataLayer.Context;
using IdentityManagement.DomainClasses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.Services
{
    public class RolesService : IRolesService
    {
        private readonly DbSet<Role> _roles;
        private readonly DbSet<User> _users;
        private readonly DbSet<UserRole> _userRoles;
        private readonly IUnitOfWork _uow;

        public RolesService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));

            _roles = _uow.Set<Role>();
            _users = _uow.Set<User>();
            _userRoles = _uow.Set<UserRole>();
        }

        public Task<List<Role>> FindUserRolesAsync(int userId)
        {
            var userRolesQuery =
                from role in _roles
                from userRole in role.UserRoles
                where userRole.UserId == userId
                select role;

            return userRolesQuery.OrderBy(x => x.Name).ToListAsync();
        }

        public Task<List<User>> FindUsersInRoleAsync(string roleName)
        {
            var roleUserIdsQuery =
                from role in _roles
                where role.Name == roleName
                from userRole in role.UserRoles
                select userRole.UserId;

            return _users
                .Where(user => roleUserIdsQuery.Contains(user.Id))
                .ToListAsync();
        }

        public async Task<bool> IsUserInRole(int userId, string roleName)
        {
            var matchedRole = await (
                from role in _roles
                where role.Name == roleName
                from userRole in role.UserRoles
                where userRole.UserId == userId
                select role
            ).FirstOrDefaultAsync();

            return matchedRole != null;
        }

        public Task<List<Role>> GetAllRolesAsync()
        {
            return _roles.OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<bool> AddUserToRoleAsync(int userId, int roleId)
        {
            var user = await _users.FirstOrDefaultAsync(x => x.Id == userId);
            var role = await _roles.FirstOrDefaultAsync(x => x.Id == roleId);

            if (user == null || role == null)
                return false;

            var alreadyExists = await _userRoles.AnyAsync(
                x => x.UserId == userId && x.RoleId == roleId);

            if (alreadyExists)
                return false;

            await _userRoles.AddAsync(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });

            user.SerialNumber = Guid.NewGuid().ToString("N");

            await _uow.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveUserFromRoleAsync(int userId, int roleId)
        {
            var user = await _users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return false;

            var userRole = await _userRoles.FirstOrDefaultAsync(
                x => x.UserId == userId && x.RoleId == roleId);

            if (userRole == null)
                return false;

            _userRoles.Remove(userRole);

            user.SerialNumber = Guid.NewGuid().ToString("N");

            await _uow.SaveChangesAsync();

            return true;
        }
    }
}