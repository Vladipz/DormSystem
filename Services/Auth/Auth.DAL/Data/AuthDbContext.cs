using Auth.DAL.Entities;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.DAL.Data
{
    public class AuthDbContext : IdentityDbContext<User, Role, Guid>
    {
        public DbSet<AuthCode> AuthCodes { get; set; } = null!;

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        public DbSet<LinkCode> LinkCodes { get; set; } = null!;

        // Parameterless constructor for EF Core design-time tools
        public AuthDbContext()
        {
        }

        public AuthDbContext(DbContextOptions<AuthDbContext> options)
          : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // This is only used at design-time for migrations
            // At runtime, DI configuration from Program.cs takes precedence
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=auth-db;Username=postgres;Password=postgres");
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        }
    }
}