using IdentityManagement.DataLayer.Context;

namespace IdentityManagement.WebApp;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureLogging(builder.Logging, builder.Environment, builder.Configuration);
        ConfigureServices(builder.Services, builder.Configuration);
        

        var app = builder.Build();


       
        app.MapGet("/", () => "Hello World!");

        app.Run();

        void ConfigureLogging(ILoggingBuilder loggingBuilder, IHostEnvironment environment, IConfiguration configuration)
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddDebug();

            if (environment.IsDevelopment())
            {
                loggingBuilder.AddConsole();
            }

            loggingBuilder.AddFile("Logs/app-{Date}.txt", minimumLevel: LogLevel.Debug);

            loggingBuilder.AddConfiguration(configuration.GetSection(key: "Logging"));
        }

        void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IUnitOfWork, IdentityManagementDbContext>();
        }
    }
}
