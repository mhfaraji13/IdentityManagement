using IdentityManagement.DataLayer.Context;
using IdentityManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
ConfigureLogging(builder.Logging, builder.Environment, builder.Configuration);
ConfigureServices(builder.Services, builder.Configuration);
var webApp = builder.Build();

ConfigureMiddlewares(webApp, webApp.Environment);

ConfigureEndpoints(webApp);

ConfigureDatabase(webApp);

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
    services.AddScoped<IDbInitializerService, DbInitializerService>();

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

    services.AddAuthorization(options =>
    {
        options.AddPolicy(CustomRoles.Admin, policy => policy.RequireRole(CustomRoles.Admin));
        options.AddPolicy(CustomRoles.User, policy => policy.RequireRole(CustomRoles.User));
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

    services.AddControllersWithViews();
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

void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
{
    if(!env.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/plain; charset=utf-8";

        await context.Response.WriteAsync(
            "An unexpected error occurred.");
    }));
    app.UseStatusCodePages();

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();

    app.UseAuthorization();
}

void ConfigureEndpoints(IApplicationBuilder app)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    });
}

void ConfigureDatabase(IApplicationBuilder app)
{
    var scopeFactory = app.ApplicationServices
        .GetRequiredService<IServiceScopeFactory>();

     var scope = scopeFactory.CreateScope();

    var dbInitializer = scope.ServiceProvider
        .GetRequiredService<IDbInitializerService>();

    dbInitializer.Initialize();

    dbInitializer.SeedData();
}



