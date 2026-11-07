using IdentityManagement.DataLayer.Context;
using IdentityManagement.DomainClasses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.Services
{
    public class UsersService : IUsersService
    {
        private readonly ISecurityService _securityService;
        private readonly IUnitOfWork _uow;
        private readonly DbSet<User> _users;


        public UsersService(IUnitOfWork uow, ISecurityService securityService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _users = _uow.Set<User>();
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        public Task<User?> GetUserForEditAsync(int userId)
        {
            return _users.FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = await _users.FirstOrDefaultAsync(
                x => x.Id == user.Id);

            if (existingUser == null)
            {
                return false;
            }

            var usernameExists = await _users.AnyAsync(
                x => x.UserName == user.UserName && x.Id != user.Id);

            if (usernameExists)
            {
                return false;
            }

            existingUser.UserName = user.UserName;
            existingUser.DisplayName = user.DisplayName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.IsActive = user.IsActive;

            await _uow.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _users.FirstOrDefaultAsync(
                x => x.Id == userId);

            if (user == null)
            {
                return false;
            }

            _users.Remove(user);

            await _uow.SaveChangesAsync();

            return true;
        }

        public Task<User?> CheckCredsAsync(string username, string password)
        {
            var passwordHash = _securityService.GetSha256Hash(password);
            return _users.FirstOrDefaultAsync(x => x.UserName == username && x.Password == passwordHash);
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            var userExists = await _users.AnyAsync(
                x => x.UserName == user.UserName);

            if (userExists)
            {
                return false;
            }

            user.Password = _securityService.GetSha256Hash(user.Password);

            user.IsActive = true;
            user.SerialNumber = Guid.NewGuid().ToString("N");

            await _users.AddAsync(user);

            await _uow.SaveChangesAsync();

            return true;
        }

        public ValueTask<User?> FindUserAsync(int userId)
        {
            return _users.FindAsync(userId);
        }

        public Task<List<User>> GetAllUsersAsync()
        {
            return _users.ToListAsync();
        }

        public async Task<string?> GetSerialNumberAsync(int userId)
        {
            var user = await FindUserAsync(userId);
            return user?.SerialNumber;
        }

        public async Task UpdateUserLastActivityDateAsync(int userId)
        {
            var user = await FindUserAsync(userId);
            if (user is null)
            {
                return;
            }
            if (user.LastLoggedIn != null)
            {
                var updateLastActivityDate = TimeSpan.FromMinutes(value: 2);
                var currentUtc = DateTimeOffset.UtcNow;
                var timeElapsed = currentUtc.Subtract(user.LastLoggedIn.Value);

                if (timeElapsed < updateLastActivityDate)
                {
                    return;
                }
            }

            user.LastLoggedIn = DateTimeOffset.UtcNow;
            await _uow.SaveChangesAsync();
        }
    }
}
