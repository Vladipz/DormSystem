using System.ComponentModel.DataAnnotations;

namespace Auth.API.Models
{
    public class ValidateLinkCodeRequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;
    }
}