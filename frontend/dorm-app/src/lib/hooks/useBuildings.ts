import { BuildingService } from "@/lib/services/buildingService";
import {
    CreateBuildingRequest,
    UpdateBuildingRequest,
} from "@/lib/types/building";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

// Fetch all buildings
export function useBuildings(page = 1, pageSize = 100, isActive = true) {
  return useQuery({
    queryKey: ["buildings", { page, pageSize, isActive }],
    queryFn: () => BuildingService.getAllBuildings(page, pageSize, isActive),
    staleTime: 5 * 60 * 1000, // 5 хвилин - не перезапитувати заново
  });
}

// Fetch one building by id
export function useBuildingById(id: string) {
  return useQuery({
    queryKey: ["buildings", id],
    queryFn: () => BuildingService.getBuildingById(id),
    enabled: !!id, // тільки якщо id є
    staleTime: 5 * 60 * 1000,
  });
}

// Create new building
export function useCreateBuilding() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateBuildingRequest) =>
      BuildingService.createBuilding(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["buildings"] });
    },
  });
}

// Update building
export function useUpdateBuilding() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateBuildingRequest) =>
      BuildingService.updateBuilding(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["buildings"] });
    },
  });
}

// Delete building
export function useDeleteBuilding() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => BuildingService.deleteBuilding(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["buildings"] });
    },
  });
}
