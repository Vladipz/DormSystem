namespace Events.API.Contracts
{
    public sealed class CreateEventRequest
    {
        public string Name { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Location { get; set; } = string.Empty;

        public int? NumberOfAttendees { get; set; }

        public bool IsPublic { get; set; }
    }
}