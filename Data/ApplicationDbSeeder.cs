using Lab8_PhamVanTung_2324801030079.Constants;
using Lab8_PhamVanTung_2324801030079.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lab8_PhamVanTung_2324801030079.Data;

public sealed class ApplicationDbSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApplicationDbSeeder> _logger;

    public ApplicationDbSeeder(
        ApplicationDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<ApplicationDbSeeder> logger)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await _dbContext.Database.MigrateAsync();

        foreach (var role in RoleConstants.All)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
                EnsureSucceeded(roleResult, $"create role '{role}'");
            }
        }

        await EnsureUserAsync(
            email: _configuration["SeedData:Admin:Email"] ?? "admin@lab.local",
            password: _configuration["SeedData:Admin:Password"] ?? "Admin@123!",
            role: RoleConstants.Admin);

        await EnsureUserAsync(
            email: _configuration["SeedData:Engineer:Email"] ?? "engineer@lab.local",
            password: _configuration["SeedData:Engineer:Password"] ?? "Engineer@123!",
            role: RoleConstants.Engineer);

        await EnsureUserAsync(
            email: _configuration["SeedData:User:Email"] ?? "user@lab.local",
            password: _configuration["SeedData:User:Password"] ?? "User@123!",
            role: RoleConstants.User);
    }

    private async Task EnsureUserAsync(string email, string password, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, password);
            EnsureSucceeded(createResult, $"create user '{email}'");
            _logger.LogInformation("Seed user {Email} created successfully.", email);
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            EnsureSucceeded(roleResult, $"assign role '{role}' to '{email}'");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operationName)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Unable to {operationName}. {errors}");
    }
}
