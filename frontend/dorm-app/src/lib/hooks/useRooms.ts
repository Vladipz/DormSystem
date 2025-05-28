import { RoomService } from "@/lib/services/roomService";
import { CreateRoomRequest, UpdateRoomRequest } from "@/lib/types/room";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

export function useRooms(
  buildingId?: string,
  enabled = true,
  blockId?: string,
) {
  return useQuery({
    queryKey: ["rooms", { buildingId, blockId }],
    queryFn: () => RoomService.getAllRooms(1, 100, blockId, buildingId),
    enabled: enabled,
    staleTime: 5 * 60 * 1000,
  });
}

//userooms with only blockless
export function useRoomsBlockless(
  buildingId?: string,
  enabled = true,
  blockId?: string,
  floorId?: string,
) {
  return useQuery({
    queryKey: ["rooms", { buildingId, blockId, floorId }],
    queryFn: () => RoomService.getAllRooms(1, 100, blockId, buildingId, floorId, true),
    enabled: enabled,
  });
}
//userooms with all rooms on a floor
export function useRoomsOnFloor(floorId: string) {
  return useQuery({
    queryKey: ["rooms", { floorId }],
    queryFn: () => RoomService.getAllRooms(1, 100, undefined, undefined, floorId, false),
    enabled: !!floorId,
  });
}
export function useRoomById(id: string) {
  return useQuery({
    queryKey: ["room", id],
    queryFn: () => RoomService.getRoomById(id),
    enabled: !!id,
  });
}

export function useCreateRoom() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateRoomRequest) => RoomService.createRoom(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
  });
}

export function useUpdateRoom() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateRoomRequest) => RoomService.updateRoom(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
  });
}

export function useDeleteRoom() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => RoomService.deleteRoom(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
  });
}
