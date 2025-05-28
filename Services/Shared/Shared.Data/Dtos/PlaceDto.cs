namespace Shared.Data.Dtos;

public record PlaceDto(
    Guid Id,
    Guid RoomId,
    int Index,
    bool IsOccupied,
    DateTime? MovedInAt,
    string RoomLabel,
    Guid? OccupiedByUserId); 