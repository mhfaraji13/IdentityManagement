using IdentityManagement.DataLayer.Context;
using IdentityManagement.Services;
var builder = WebApplication.CreateBuilder(args);
ConfigureLogging(builder.Logging, builder.Environment, builder.Configuration);
ConfigureServices(builder.Services, builder.Configuration);
var webApp = builder.Build();



webApp.MapGet("/", (IDeviceDetectionService deviceDetection) =>
    deviceDetection.GetCurrentRequestDeviceDetails());

webApp.Run();


void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddHttpContextAccessor();
    services.AddSingleton<ISecurityService, SecurityService>();
    services.AddScoped<IDeviceDetectionService, DeviceDetectionService>();

    // TODO: register with AddDbContext once a connection string exists.
    // As written, this fails DI validation at startup (no DbContextOptions).
    // services.AddScoped<IUnitOfWork, IdentityManagementDbContext>();
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



