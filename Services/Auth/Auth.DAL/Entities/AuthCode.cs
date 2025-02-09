namespace Auth.DAL.Entities
{
    public class AuthCode
    {
        public Guid Id { get; set; } // Primary key

        public string Code { get; set; } = string.Empty; // The generated code (could be base64 encoded)

        public string CodeChallenge { get; set; } = string.Empty; // The code challenge to verify PKCE

        public Guid UserId { get; set; } // Foreign key to the user

        public DateTime Created { get; set; } // When it was generated

        public DateTime Expires { get; set; } // Optional: the expiry time for the auth code
    }
}