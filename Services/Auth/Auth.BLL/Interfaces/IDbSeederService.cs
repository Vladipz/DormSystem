namespace Auth.BLL.Interfaces
{
    /// <summary>
    /// Service for seeding initial data into the database.
    /// </summary>
    public interface IDbSeederService
    {
        /// <summary>
        /// Seeds the database with initial roles and admin user.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SeedDatabaseAsync();
    }
}