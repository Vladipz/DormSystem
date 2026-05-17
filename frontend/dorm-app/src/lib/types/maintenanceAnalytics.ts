import type { MaintenancePriority, MaintenanceStatus } from "./maintenanceTicket";
import type { UserShortResponse } from "./sharedDtos";

export interface MaintenanceAnalyticsFilters {
  buildingId?: string;
  dateFrom?: string;
  dateTo?: string;
  status?: MaintenanceStatus;
  priority?: MaintenancePriority;
  assignedToId?: string;
}

export interface MaintenanceHeatmapResponse {
  buildingId: string;
  buildingName: string;
  dateFrom?: string | null;
  dateTo?: string | null;
  maxTicketsCount: number;
  cells: MaintenanceHeatmapCellResponse[];
}

export interface MaintenanceHeatmapCellResponse {
  floorId: string;
  floorNumber: number;
  blockId: string;
  blockLabel: string;
  ticketsCount: number;
  openCount: number;
  inProgressCount: number;
  resolvedCount: number;
  resolvedPercentage: number;
  mostFrequentPriority?: MaintenancePriority | null;
}

export interface MaintenanceDrilldownParams extends MaintenanceAnalyticsFilters {
  buildingId: string;
  floorId: string;
  blockId: string;
}

export interface MaintenanceDrilldownResponse {
  buildingId: string;
  buildingName: string;
  floorId: string;
  floorNumber: number;
  blockId: string;
  blockLabel: string;
  summary: MaintenanceDrilldownSummaryResponse;
  tickets: MaintenanceDrilldownTicketResponse[];
  roomBars: MaintenanceRoomBarResponse[];
  timeline: MaintenanceTimelinePointResponse[];
  diagnosticMessage: string;
}

export interface MaintenanceDrilldownSummaryResponse {
  ticketsCount: number;
  openCount: number;
  inProgressCount: number;
  resolvedCount: number;
  resolvedPercentage: number;
  averageDaysInWork: number;
  mostLoadedRoomLabel?: string | null;
  mostFrequentPriority?: MaintenancePriority | null;
}

export interface MaintenanceDrilldownTicketResponse {
  id: string;
  roomId: string;
  roomLabel: string;
  title: string;
  description: string;
  createdAt: string;
  resolvedAt?: string | null;
  assignedTo?: UserShortResponse | null;
  status: MaintenanceStatus;
  priority: MaintenancePriority;
  daysInWork: number;
}

export interface MaintenanceRoomBarResponse {
  roomId: string;
  roomLabel: string;
  ticketsCount: number;
}

export interface MaintenanceTimelinePointResponse {
  label: string;
  periodStart: string;
  ticketsCount: number;
}
