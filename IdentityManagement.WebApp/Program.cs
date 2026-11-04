using IdentityManagement.DataLayer.Context;
using IdentityManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;


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
    services.AddScoped<ICookieValidatorService, CookieValidatorService>();
    services.AddScoped<ISecurityService, SecurityService>();

    services.AddDbContext<IdentityManagementDbContext>(options =>
    {
        var connectionString = configuration.GetConnectionString(name: "DefaultConnection");
        options.UseSqlServer(connectionString, serverDbContextOptionsBuilder =>
        {
            var minutes = (int)TimeSpan.FromMinutes(value: 3).TotalSeconds;
            serverDbContextOptionsBuilder.CommandTimeout(minutes);
            serverDbContextOptionsBuilder.EnableRetryOnFailure();
        });
    });

    services.AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.SlidingExpiration = false;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";

        options.Cookie.Name = ".identitymanagement.cookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = context =>
            {
                var cookieValidatorService = context.HttpContext.RequestServices.GetRequiredService<ICookieValidatorService>();
                return cookieValidatorService.ValidateAsync(context);
            }
        };
    });
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



