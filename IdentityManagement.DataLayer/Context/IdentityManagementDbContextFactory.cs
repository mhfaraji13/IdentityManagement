using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityManagement.DataLayer.Context
{
    public class IdentityManagementDbContextFactory : IDesignTimeDbContextFactory<IdentityManagementDbContext>
    {
        public IdentityManagementDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            WriteLine($"Using `{basePath}` as the BasePath");

            var configuration = new ConfigurationBuilder().SetBasePath(basePath)
                .AddJsonFile(path: "appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<IdentityManagementDbContext>();
            var connectionString = configuration.GetConnectionString(name: "DefaultConnection");
            builder.UseSqlServer(connectionString);

            return new IdentityManagementDbContext(builder.Options);
        }
    }
}
