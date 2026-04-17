using ASC.Model.BaseTypes;

using ASCwed.Cofiguration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace ASC.Web.Data
{
    public class IdentitySeed : IIdentitySeed
    {
        public async Task Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<ApplicationSettings> options)
        {
            var settings = options.Value;
            var roles = (settings.Roles ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var role in roles)
            {
                try
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        IdentityRole storageRole = new IdentityRole
                        {
                            Name = role
                        };
                        IdentityResult roleResult = await roleManager.CreateAsync(storageRole);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            if (!string.IsNullOrWhiteSpace(settings.AdminEmail) &&
                !string.IsNullOrWhiteSpace(settings.AdminPassword))
            {
                var admin = await userManager.FindByEmailAsync(settings.AdminEmail);
                if (admin == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = settings.AdminName,
                        Email = settings.AdminEmail,
                        EmailConfirmed = true
                    };

                    IdentityResult result = await userManager.CreateAsync(user, settings.AdminPassword);
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", settings.AdminEmail));
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(settings.EngineerEmail) &&
                !string.IsNullOrWhiteSpace(settings.EngineerPassword))
            {
                var engineer = await userManager.FindByEmailAsync(settings.EngineerEmail);
                if (engineer == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = settings.EngineerName,
                        Email = settings.EngineerEmail,
                        EmailConfirmed = true,
                        LockoutEnabled = false
                    };

                    IdentityResult result = await userManager.CreateAsync(user, settings.EngineerPassword);
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", settings.EngineerEmail));
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, Roles.Engineer.ToString());
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(settings.UserEmail) &&
                !string.IsNullOrWhiteSpace(settings.UserPassword))
            {
                var appUser = await userManager.FindByEmailAsync(settings.UserEmail);
                if (appUser == null)
                {
                    IdentityUser user = new IdentityUser
                    {
                        UserName = settings.UserName,
                        Email = settings.UserEmail,
                        EmailConfirmed = true,
                        LockoutEnabled = false
                    };

                    IdentityResult result = await userManager.CreateAsync(user, settings.UserPassword);
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", settings.UserEmail));
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("IsActive", "True"));
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, Roles.User.ToString());
                    }
                }
            }
        }
    }
}
