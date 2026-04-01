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
        private static readonly Guid TestStudentOneId = new Guid("44444444-4444-4444-4444-444444444444");
        private static readonly Guid TestStudentTwoId = new Guid("55555555-5555-5555-5555-555555555555");

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
            await EnsureUserAsync(
                RegularUserId,
                "student@dorm.com",
                "Student",
                "User",
                "Student123!",
                "Student");

            await EnsureUserAsync(
                TestStudentOneId,
                "student1.test@dorm.com",
                "Test",
                "StudentOne",
                "Student123!",
                "Student");

            await EnsureUserAsync(
                TestStudentTwoId,
                "student2.test@dorm.com",
                "Test",
                "StudentTwo",
                "Student123!",
                "Student");

            await EnsureUserAsync(
                MaintenanceStaffId,
                "maintenance@dorm.com",
                "Maintenance",
                "Staff",
                "Maintenance123!",
                "Security");
        }

        private async Task EnsureUserAsync(
            Guid userId,
            string email,
            string firstName,
            string lastName,
            string password,
            string role)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                user = new User
                {
                    Id = userId,
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName,
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Error creating seeded user {email}: {string.Join(", ", result.Errors.Select(x => x.Description))}");
                }
            }

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            var needsUpdate = false;
            if (user.FirstName != firstName)
            {
                user.FirstName = firstName;
                needsUpdate = true;
            }

            if (user.LastName != lastName)
            {
                user.LastName = lastName;
                needsUpdate = true;
            }

            if (user.Email != email)
            {
                user.Email = email;
                needsUpdate = true;
            }

            if (user.UserName != email)
            {
                user.UserName = email;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await _userManager.UpdateAsync(user);
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
