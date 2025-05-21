import { useQuery } from "@tanstack/react-query";
import { dormitoryService } from "../services/dormitoryService";

export function useDormitories() {
  return useQuery({
    queryKey: ["dormitories"],
    queryFn: () => dormitoryService.getDormitories(),
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  });
} 