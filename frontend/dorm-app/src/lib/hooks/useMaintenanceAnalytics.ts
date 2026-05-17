import { maintenanceAnalyticsService } from "@/lib/services/maintenanceAnalyticsService";
import type {
  MaintenanceAnalyticsFilters,
  MaintenanceDrilldownParams,
} from "@/lib/types/maintenanceAnalytics";
import { useQuery } from "@tanstack/react-query";

export const MAINTENANCE_ANALYTICS_KEY = "maintenanceAnalytics";

export function useMaintenanceHeatmap(params: MaintenanceAnalyticsFilters) {
  return useQuery({
    queryKey: [MAINTENANCE_ANALYTICS_KEY, "heatmap", params],
    queryFn: () => maintenanceAnalyticsService.getHeatmap(params),
    enabled: !!params.buildingId,
  });
}

export function useMaintenanceDrilldown(params?: MaintenanceDrilldownParams) {
  return useQuery({
    queryKey: [MAINTENANCE_ANALYTICS_KEY, "drilldown", params],
    queryFn: () => maintenanceAnalyticsService.getDrilldown(params!),
    enabled: !!params?.buildingId && !!params.floorId && !!params.blockId,
  });
}
