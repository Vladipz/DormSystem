import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { FloorService } from "@/lib/services/floorService";
import {
  CreateFloorRequest,
  UpdateFloorRequest
} from "@/lib/types/floor";

// Fetch all floors (by buildingId)
export function useFloors(buildingId?: string, enabled = true) {
  return useQuery({
    queryKey: ["floors", buildingId],
    queryFn: () => FloorService.getAllFloors(buildingId),
    enabled: enabled && !!buildingId,
    staleTime: 5 * 60 * 1000, // 5 хвилин кеш
  });
}

// Fetch one floor by ID
export function useFloorById(id: string) {
  return useQuery({
    queryKey: ["floor", id],
    queryFn: () => FloorService.getFloorById(id),
    enabled: !!id,
  });
}

// Create floor
export function useCreateFloor() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateFloorRequest) => FloorService.createFloor(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["floors"] });
    },
  });
}

// Update floor
export function useUpdateFloor() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateFloorRequest }) =>
      FloorService.updateFloor(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["floors"] });
    },
  });
}

// Delete floor
export function useDeleteFloor() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => FloorService.deleteFloor(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["floors"] });
    },
  });
}
