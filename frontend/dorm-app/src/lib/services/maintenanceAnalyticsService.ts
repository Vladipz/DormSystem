import { axiosClient } from "@/lib/utils/axios-client";

import type {
  MaintenanceAnalyticsFilters,
  MaintenanceDrilldownParams,
  MaintenanceDrilldownResponse,
  MaintenanceHeatmapResponse,
} from "@/lib/types/maintenanceAnalytics";

const BASE_URL = "/maintenance-analytics";

export const maintenanceAnalyticsService = {
  async getHeatmap(params: MaintenanceAnalyticsFilters) {
    const { data } = await axiosClient.get<MaintenanceHeatmapResponse>(
      `${BASE_URL}/heatmap`,
      { params },
    );
    return data;
  },

  async getDrilldown(params: MaintenanceDrilldownParams) {
    const { data } = await axiosClient.get<MaintenanceDrilldownResponse>(
      `${BASE_URL}/drilldown`,
      { params },
    );
    return data;
  },
};
