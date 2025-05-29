using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Auth.DAL.Entities
{
    public class User : IdentityUser<Guid>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid Id
        {
            get => base.Id;
            set => base.Id = value;
        }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? AvatarId { get; set; }

        public ICollection<RefreshToken> RefreshTokens { get; init; } = [];
    }
}