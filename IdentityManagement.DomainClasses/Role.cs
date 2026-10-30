using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.DomainClasses
{
    public class Role
    {
        public Role() => UserRoles = [];

        public int Id { get; set; }
        public required string Name { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
