using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.Web.Data;
using ASCwed.Cofiguration;
using ASCwed.Data;
using ASCwed.Services;
using ASCwed.Services.Navigation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;


namespace ASCwed
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMyDependencyGroup(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.LogoutPath = "/Identity/Account/Logout";
                options.SlidingExpiration = true;
            });

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = configuration["Google:Identity:ClientId"]!;
                    options.ClientSecret = configuration["Google:Identity:ClientSecret"]!;
                });

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddDistributedMemoryCache();
            services.AddMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".ASCwed.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            services.Configure<ApplicationSettings>(
                configuration.GetSection("AppSettings"));

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddScoped<DbContext, ApplicationDbContext>();
            services.AddTransient<IIdentitySeed, IdentitySeed>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<INavigationCacheOperations, NavigationCacheOperations>();

            return services;
        }
    }
}