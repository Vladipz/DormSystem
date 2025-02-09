namespace Auth.DAL.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } // Primary key

        public string Token { get; set; } = string.Empty; // The token string

        public Guid UserId { get; set; } // Foreign key to the user

        public User? User { get; set; } // Navigation property to the user

        public DateTime Expires { get; set; } // When it expires

        public bool IsRevoked { get; set; } // If it was revoked
    }
}