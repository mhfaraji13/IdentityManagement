using IdentityManagement.DataLayer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IdentityManagement.DomainClasses;

namespace IdentityManagement.Services
{
    public class DbInitializerService(
     IServiceScopeFactory scopeFactory) : IDbInitializerService
    {
        private readonly IServiceScopeFactory _scopeFactory =
            scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

        public void Initialize()
        {
            using var serviceScope = _scopeFactory.CreateScope();

            using var context =
                serviceScope.ServiceProvider.GetService<IdentityManagementDbContext>()
                ?? throw new InvalidOperationException("context is null");

            context.Database.Migrate();
        }

        public void SeedData()
        {
            using var serviceScope = _scopeFactory.CreateScope();

            using var context =
                serviceScope.ServiceProvider.GetService<IdentityManagementDbContext>()
                ?? throw new InvalidOperationException("context is null");

             var security = serviceScope.ServiceProvider.GetService<ISecurityService>() 
                ?? throw new InvalidOperationException("context is null");

            // Add default roles

            var adminRole = context.Roles
                .FirstOrDefault(role => role.Name == CustomRoles.Admin);

            if (adminRole is null)
            {
                adminRole = new Role
                {
                    Name = CustomRoles.Admin
                };

                context.Roles.Add(adminRole);
            }

            var userRole = context.Roles
                .FirstOrDefault(role => role.Name == CustomRoles.User);

            if (userRole is null)
            {
                userRole = new Role
                {
                    Name = CustomRoles.User
                };

                context.Roles.Add(userRole);
            }

            context.SaveChanges();
            // Add Admin user
            if (!context.Users.Any())
            {
                var adminUser = new IdentityManagement.DomainClasses.User
                {
                    UserName = "mohammadhosseinadmin",
                    DisplayName = "Administrator",
                    IsActive = true,
                    LastLoggedIn = null,
                    Password = security.GetSha256Hash(input: "mhadmin123"),
                    SerialNumber = Guid.NewGuid().ToString(format: "N")
                };

                context.Users.Add(adminUser);
                context.SaveChanges();

                context.UserRoles.Add(new UserRole
                {
                    User = adminUser,
                    Role = adminRole
                });

                context.UserRoles.Add(new UserRole
                {
                    User = adminUser,
                    Role = userRole
                });

                context.SaveChanges();
            }
        }
    }
}
