using Lab8_PhamVanTung_2324801030079.Business.Cache;
using Lab8_PhamVanTung_2324801030079.Data;
using Lab8_PhamVanTung_2324801030079.Middleware;
using Lab8_PhamVanTung_2324801030079.Models;
using Lab8_PhamVanTung_2324801030079.Models.Cache;
using Lab8_PhamVanTung_2324801030079.Services;
using Lab8_PhamVanTung_2324801030079.Services.Navigation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace Lab8_PhamVanTung_2324801030079.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddMemoryCache();

        // Cấu hình Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = "localhost:6379";
            options.InstanceName = "Lab8_";
        });

        services.AddHttpContextAccessor();
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddDefaultUI();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Home/Privacy";
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.SlidingExpiration = true;
        });

        services.AddAuthorization();
        services.AddSession(options =>
        {
            options.Cookie.Name = ".Lab8.Session";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.IdleTimeout = TimeSpan.FromMinutes(30);
        });

        services.AddControllersWithViews();
        services.AddRazorPages();


        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<ApplicationDbSeeder>();
        services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();


        services.AddScoped<IMasterDataCacheOperations, MasterDataCacheOperations>();
      

        return services;
    }

    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseSession();
        app.UseMiddleware<CurrentUserSessionMiddleware>();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Dashboard}/{action=Dashboard}/{id?}");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        return app;
    }
}