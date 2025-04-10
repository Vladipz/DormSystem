using Auth.BLL.Interfaces;
using Auth.BLL.Models;
using Auth.DAL.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Auth.BLL.Services
{
    public class DbSeederService : IDbSeederService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IOptions<AdminSettings> _adminSettings;

        public DbSeederService(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<AdminSettings> adminSettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _adminSettings = adminSettings;
        }

        public async Task SeedDatabaseAsync()
        {
            // Seed roles first
            await SeedRolesAsync();

            // Create admin user if it doesn't exist
            var adminEmail = _adminSettings.Value.Email;
            if (!string.IsNullOrEmpty(adminEmail))
            {
                var existingUser = await _userManager.FindByEmailAsync(adminEmail);

                if (existingUser == null)
                {
                    var adminUser = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = _adminSettings.Value.FirstName,
                        LastName = _adminSettings.Value.LastName,
                    };

                    var password = _adminSettings.Value.Password;
                    if (!string.IsNullOrEmpty(password))
                    {
                        var result = await _userManager.CreateAsync(adminUser, password);

                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(adminUser, "Admin");
                        }
                    }
                }
                else if (!await _userManager.IsInRoleAsync(existingUser, "Admin"))
                {
                    // Ensure the user is in Admin role if they already exist
                    await _userManager.AddToRoleAsync(existingUser, "Admin");

                    // Update FirstName and LastName if they are empty
                    if (string.IsNullOrEmpty(existingUser.FirstName) && !string.IsNullOrEmpty(_adminSettings.Value.FirstName))
                    {
                        existingUser.FirstName = _adminSettings.Value.FirstName;
                    }

                    if (string.IsNullOrEmpty(existingUser.LastName) && !string.IsNullOrEmpty(_adminSettings.Value.LastName))
                    {
                        existingUser.LastName = _adminSettings.Value.LastName;
                    }

                    if (!string.IsNullOrEmpty(_adminSettings.Value.FirstName) || !string.IsNullOrEmpty(_adminSettings.Value.LastName))
                    {
                        await _userManager.UpdateAsync(existingUser);
                    }
                }
            }
        }

        private async Task SeedRolesAsync()
        {
            string[] roleNames =
            [
                "Admin",
                "StudentCouncil",
                "Student",
                "Security",
            ];

            foreach (var roleName in roleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    // Create the roles and seed them to the database
                    var result = await _roleManager.CreateAsync(new Role { Name = roleName });
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Error creating role {roleName}: {string.Join(", ", result.Errors)}");
                    }
                }
            }
        }
    }
}