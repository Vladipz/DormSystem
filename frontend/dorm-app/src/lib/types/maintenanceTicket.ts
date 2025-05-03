import { UserShortResponse } from "./sharedDtos";

export type MaintenanceStatus = "Open" | "InProgress" | "Resolved";
export type MaintenancePriority = "Low" | "Medium" | "High";

export interface ShortRoomResponse {
  id: string;
  label: string;
}

export interface CreateMaintenanceTicketRequest {
  roomId: string;
  title: string;
  description: string;
  reporterById: string;
  assignedToId?: string | null;
  priority: MaintenancePriority;
}

export interface CreateMaintenanceTicketResponse {
  id: string;
}

export interface MaintenanceTicketResponse {
  id: string;
  room: ShortRoomResponse;
  title: string;
  description: string;
  status: MaintenanceStatus;
  createdAt: string;
  resolvedAt?: string | null;
  reporter: UserShortResponse;
  assignedTo?: UserShortResponse | null;
  priority: MaintenancePriority;
}

export interface MaintenanceTicketDetailsResponse {
  id: string;
  room: ShortRoomResponse;
  title: string;
  description: string;
  status: MaintenanceStatus;
  createdAt: string;
  resolvedAt?: string | null;
  reporterById: string;
  assignedToId?: string | null;
  priority: MaintenancePriority;
}

export interface UpdateMaintenanceTicketRequest {
  ticketId: string;
  title: string;
  description: string;
  assignedToId?: string | null;
  priority: MaintenancePriority;
}

export interface UpdatedMaintenanceTicketResponse {
  id: string;
}

export interface ChangeMaintenanceTicketStatusRequest {
  ticketId: string;
  newStatus: MaintenanceStatus;
}

export interface UpdatedMaintenanceTicketStatusResponse {
  id: string;
  newStatus: MaintenanceStatus;
}

export interface DeletedMaintenanceTicketResponse {
  id: string;
}
