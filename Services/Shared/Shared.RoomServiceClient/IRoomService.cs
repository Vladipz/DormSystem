using ErrorOr;

using Shared.Data.Dtos;


namespace Shared.RoomServiceClient;

public interface IRoomService
{
    //dont use this because need to change return type
    Task<ErrorOr<List<RoomDto>>> GetRoomsInfoByDormitoryIdAsync(Guid dormitoryId, CancellationToken ct = default);
    
    Task<ErrorOr<List<RoomDto>>> GetRoomsForInspectionAsync(Guid dormitoryId, bool includeSpecial = true, CancellationToken ct = default);
}
