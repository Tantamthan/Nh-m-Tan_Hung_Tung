using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ASCwed.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var projectPath = Path.GetFullPath(Path.Combine(basePath, "..", "ASCwed"));
            if (!File.Exists(Path.Combine(basePath, "appsettings.json"))
                && File.Exists(Path.Combine(projectPath, "appsettings.json")))
            {
                // Hỗ trợ chạy dotnet ef từ thư mục tạm nhưng vẫn đọc cấu hình của web project.
                basePath = projectPath;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            optionsBuilder.UseApplicationServiceProvider(
                new ServiceCollection()
                    .AddIdentityCore<IdentityUser>(options =>
                    {
                        options.Stores.MaxLengthForKeys = 128;
                    })
                    .AddRoles<IdentityRole>()
                    .Services
                    .BuildServiceProvider());

            return new ApplicationDbContext(optionsBuilder.Options, migrateDatabase: false);
        }
    }
}
