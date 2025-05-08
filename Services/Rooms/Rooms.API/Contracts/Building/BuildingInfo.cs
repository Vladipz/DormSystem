namespace Rooms.API.Contracts.Building
{
    public sealed class BuildingInfo
    {
        public Guid Id { get; init; }

        public string Label { get; init; } = string.Empty;
    }
}