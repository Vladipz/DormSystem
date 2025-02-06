using Auth.DAL.Entities;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.DAL.Data
{
    public class AuthDbContext : IdentityDbContext<User, Role, Guid>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
          : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        }
    }
}