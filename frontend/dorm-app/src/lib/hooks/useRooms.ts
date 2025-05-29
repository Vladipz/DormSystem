import { RoomService } from "@/lib/services/roomService";
import {
  CreateRoomRequest,
  DeletedRoomPhotoResponse,
  UpdateRoomRequest,
  UploadRoomPhotoResponse
} from "@/lib/types/room";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

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

// Hook для завантаження фото кімнати
export function useUploadRoomPhoto() {
  const queryClient = useQueryClient();

  return useMutation<UploadRoomPhotoResponse, Error, { roomId: string; photo: File }>({
    mutationFn: ({ roomId, photo }) => RoomService.uploadRoomPhoto(roomId, photo),
    onSuccess: (data, variables) => {
      toast.success(data.message || "Room photo uploaded successfully");
      // Invalidate room details to refresh photo URLs
      queryClient.invalidateQueries({ queryKey: ["room", variables.roomId] });
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
    onError: (error) => {
      console.error("Error uploading room photo:", error);
      toast.error("Failed to upload room photo");
    },
  });
}

// Hook для видалення фото кімнати
export function useDeleteRoomPhoto() {
  const queryClient = useQueryClient();

  return useMutation<DeletedRoomPhotoResponse, Error, { roomId: string; photoId: string }>({
    mutationFn: ({ roomId, photoId }) => RoomService.deleteRoomPhoto(roomId, photoId),
    onSuccess: (data, variables) => {
      toast.success(data.message || "Room photo deleted successfully");
      // Invalidate room details to refresh photo URLs
      queryClient.invalidateQueries({ queryKey: ["room", variables.roomId] });
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    },
    onError: (error) => {
      console.error("Error deleting room photo:", error);
      toast.error("Failed to delete room photo");
    },
  });
}
