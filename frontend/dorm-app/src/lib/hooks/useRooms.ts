
import { RoomService } from "@/lib/services/roomService";
import { CreateRoomRequest, UpdateRoomRequest } from "@/lib/types/room";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

export function useRooms(blockId?: string, enabled = true) {
  return useQuery({
    queryKey: ["rooms", blockId],
    queryFn: () => RoomService.getAllRooms(1, 100, blockId),
    enabled: enabled,
    staleTime: 5 * 60 * 1000,
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
