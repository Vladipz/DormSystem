namespace Shared.Data.Dtos;

public record RoomDto(Guid Id,
                      string RoomNumber,
                      int Floor,
                      string Building,
                      bool IsSpecial);