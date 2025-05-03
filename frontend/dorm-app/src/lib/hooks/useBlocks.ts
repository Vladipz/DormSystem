import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { BlockService } from "@/lib/services/blockService";
import {
  CreateBlockRequest,
  UpdateBlockRequest
} from "@/lib/types/block";

// Fetch all blocks (by floorId)
export function useBlocks(floorId?: string, enabled = true) {
  return useQuery({
    queryKey: ["blocks", floorId],
    queryFn: () => BlockService.getAllBlocks(floorId),
    enabled: enabled && !!floorId,
    staleTime: 5 * 60 * 1000, // 5 хвилин кеш
  });
}

// Fetch one block by ID
export function useBlockById(id: string) {
  return useQuery({
    queryKey: ["block", id],
    queryFn: () => BlockService.getBlockById(id),
    enabled: !!id,
  });
}

// Create block
export function useCreateBlock() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateBlockRequest) => BlockService.createBlock(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["blocks"] });
    },
  });
}

// Update block
export function useUpdateBlock() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateBlockRequest }) =>
      BlockService.updateBlock(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["blocks"] });
    },
  });
}

// Delete block
export function useDeleteBlock() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => BlockService.deleteBlock(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["blocks"] });
    },
  });
}
