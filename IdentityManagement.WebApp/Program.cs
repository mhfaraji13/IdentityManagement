using IdentityManagement.DataLayer.Context;
using IdentityManagement.Services;


var builder = WebApplication.CreateBuilder(args);
ConfigureLogging(builder.Logging, builder.Environment, builder.Configuration);
ConfigureServices(builder.Services, builder.Configuration);
var webApp = builder.Build();



webApp.MapGet("/", () => "Hello World!");

webApp.Run();


void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{

    services.AddHttpContextAccessor();
    services.AddScoped<IUnitOfWork, IdentityManagementDbContext>();
    services.AddScoped<IDeviceDetectionService, DeviceDetectionService>();
    services.AddScoped<IUsersService, UsersService>();
    services.AddScoped<IRolesService, RolesService>();
    services.AddScoped<ISecurityService, SecurityService>();
}
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



