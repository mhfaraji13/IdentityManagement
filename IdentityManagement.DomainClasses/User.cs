using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.DomainClasses
{
    public class User
    {
        public User() => UserRoles = [];
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? DisplayName { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? LastLoggedIn { get; set; }
        public string? SerialNumber { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? PhotoPath { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
