namespace Rooms.API.Contracts.Building
{
    public sealed class CreateBuildingRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int FloorsCount { get; set; }

        public int YearBuilt { get; set; }

        public string AdministratorContact { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}