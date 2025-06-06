namespace Rooms.API.Contracts.Building
{
    public sealed class BuildingsResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int FloorsCount { get; set; }

        public int YearBuilt { get; set; }

        public bool IsActive { get; set; }
    }
}