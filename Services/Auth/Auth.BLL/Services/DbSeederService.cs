using Auth.BLL.Interfaces;
using Auth.BLL.Models;
using Auth.DAL.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Auth.BLL.Services
{
    public class DbSeederService : IDbSeederService
    {
        // Static GUIDs for seeding user references - matching the ones in Rooms.API
        private static readonly Guid AdminUserId = new Guid("11111111-1111-1111-1111-111111111111");
        private static readonly Guid RegularUserId = new Guid("22222222-2222-2222-2222-222222222222");
        private static readonly Guid MaintenanceStaffId = new Guid("33333333-3333-3333-3333-333333333333");

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

            // Create admin user with specific GUID if it doesn't exist
            await SeedAdminUserAsync();

            // Create regular users with specific GUIDs
            await SeedRegularUsersAsync();
        }

        private async Task SeedAdminUserAsync()
        {
            var adminEmail = _adminSettings.Value.Email;
            if (string.IsNullOrEmpty(adminEmail))
            {
                adminEmail = "admin@dorm.com";
            }

            // Check if admin exists by ID first
            var adminUser = await _userManager.FindByIdAsync(AdminUserId.ToString());
            if (adminUser == null)
            {
                // Also check by email as fallback
                adminUser = await _userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    // Create new admin user with predefined ID
                    adminUser = new User
                    {
                        Id = AdminUserId,
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = _adminSettings.Value.FirstName ?? "Admin",
                        LastName = _adminSettings.Value.LastName ?? "User",
                    };

                    var password = _adminSettings.Value.Password;
                    if (string.IsNullOrEmpty(password))
                    {
                        password = "Admin123!";
                    }

                    var result = await _userManager.CreateAsync(adminUser, password);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
                else if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    // Ensure the user is in Admin role if they already exist
                    await _userManager.AddToRoleAsync(adminUser, "Admin");

                    // Update FirstName and LastName if they are empty
                    if (string.IsNullOrEmpty(adminUser.FirstName) && !string.IsNullOrEmpty(_adminSettings.Value.FirstName))
                    {
                        adminUser.FirstName = _adminSettings.Value.FirstName;
                    }

                    if (string.IsNullOrEmpty(adminUser.LastName) && !string.IsNullOrEmpty(_adminSettings.Value.LastName))
                    {
                        adminUser.LastName = _adminSettings.Value.LastName;
                    }

                    if (!string.IsNullOrEmpty(_adminSettings.Value.FirstName) || !string.IsNullOrEmpty(_adminSettings.Value.LastName))
                    {
                        await _userManager.UpdateAsync(adminUser);
                    }
                }
            }
        }

        private async Task SeedRegularUsersAsync()
        {
            // Create a regular user with the Reporter1 ID if it doesn't exist
            var regularUser = await _userManager.FindByIdAsync(RegularUserId.ToString());
            if (regularUser == null)
            {
                regularUser = new User
                {
                    Id = RegularUserId,
                    UserName = "student@dorm.com",
                    Email = "student@dorm.com",
                    EmailConfirmed = true,
                    FirstName = "Student",
                    LastName = "User",
                };

                var result = await _userManager.CreateAsync(regularUser, "Student123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(regularUser, "Student");
                }
            }

            // Create a maintenance staff user if it doesn't exist
            var maintenanceUser = await _userManager.FindByIdAsync(MaintenanceStaffId.ToString());
            if (maintenanceUser == null)
            {
                maintenanceUser = new User
                {
                    Id = MaintenanceStaffId,
                    UserName = "maintenance@dorm.com",
                    Email = "maintenance@dorm.com",
                    EmailConfirmed = true,
                    FirstName = "Maintenance",
                    LastName = "Staff",
                };

                var result = await _userManager.CreateAsync(maintenanceUser, "Maintenance123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(maintenanceUser, "Security");
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