namespace Inspections.API.Entities
{
    public enum InspectionStatus
    {
        Scheduled,
        Active,
        Completed,
    }

    public enum RoomInspectionStatus
    {
        Pending,
        Confirmed,
        NotConfirmed,
        NoAccess,
    }
}