namespace Auth.DAL.Entities
{
    public class AuthCode
    {
        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the generated code (could be base64 encoded).
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the code challenge to verify PKCE.
        /// </summary>
        public string CodeChallenge { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the foreign key to the user.
        /// </summary>
        public Guid UserId { get; set; }
    }
}