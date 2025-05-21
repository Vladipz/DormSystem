import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
    CreateInspectionDto,
    inspectionService,
    ReportStyle,
    UpdateRoomStatusDto,
} from "../services/inspectionService";
import { InspectionStatus } from "../types/inspection";

// KEYS
const keys = {
  all: ["inspections"] as const,
  lists: (filters?: object) => [...keys.all, "list", filters] as const,
  detail: (id: string) => [...keys.all, "detail", id] as const,
};

// 🔍 List Inspections
export function useListInspections(filters?: {
  status?: InspectionStatus;
  type?: string;
  from?: string;
  to?: string;
  pageNumber?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: keys.lists(filters),
    queryFn: () => inspectionService.list(filters),
  });
}

// 📄 Get by ID
export function useInspection(id: string) {
  return useQuery({
    queryKey: keys.detail(id),
    queryFn: () => inspectionService.getById(id),
  });
}

// ➕ Create
export function useCreateInspection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (dto: CreateInspectionDto) => inspectionService.create(dto),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: keys.all });
    },
  });
}

// ✅ Start
export function useStartInspection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => inspectionService.start(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: keys.all });
    },
  });
}

// 🎯 Complete
export function useCompleteInspection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => inspectionService.complete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: keys.all });
    },
  });
}

// 🛠️ Update Room
export function useUpdateRoomStatus(inspectionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (params: { roomId: string; data: UpdateRoomStatusDto }) =>
      inspectionService.updateRoomStatus(
        inspectionId,
        params.roomId,
        params.data,
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: keys.detail(inspectionId) });
    },
  });
}

// 📄 Generate Report
export function useGenerateReport() {
  return useMutation({
    mutationFn: (params: { id: string; style: ReportStyle }) => 
      inspectionService.generateReport(params.id, params.style),
  });
}
