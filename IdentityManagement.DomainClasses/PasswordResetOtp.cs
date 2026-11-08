using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.DomainClasses
{
    public class PasswordResetOtp
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string CodeHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public int Attempts { get; set; }

        public bool IsUsed { get; set; }

        public User User { get; set; } = null!;
        public bool IsVerified { get; set; }
    }
}
