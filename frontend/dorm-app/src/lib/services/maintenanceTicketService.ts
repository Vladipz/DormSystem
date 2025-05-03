import { axiosClient } from "@/lib/utils/axios-client";

import type {
  CreateMaintenanceTicketRequest,
  CreateMaintenanceTicketResponse,
  MaintenanceTicketResponse,
  MaintenanceTicketDetailsResponse,
  UpdateMaintenanceTicketRequest,
  UpdatedMaintenanceTicketResponse,
  ChangeMaintenanceTicketStatusRequest,
  UpdatedMaintenanceTicketStatusResponse,
  DeletedMaintenanceTicketResponse,
  MaintenanceStatus,
  MaintenancePriority,
} from "@/lib/types/maintenanceTicket";
import { PagedResponse } from "@/lib/types/pagination";

// Base URL for maintenance tickets endpoints
const BASE_URL = "/maintenance-tickets";

export const maintenanceTicketService = {
  /**
   * Get a paginated list of maintenance tickets with optional filters
   */
  async getMaintenanceTickets(params?: {
    roomId?: string;
    status?: MaintenanceStatus;
    reporterById?: string;
    assignedToId?: string;
    priority?: MaintenancePriority;
    page?: number;
    pageSize?: number;
  }) {
    const { data } = await axiosClient.get<PagedResponse<MaintenanceTicketResponse>>(BASE_URL, { params });
    return data;
  },

  /**
   * Get a maintenance ticket by ID
   */
  async getMaintenanceTicketById(id: string) {
    const { data } = await axiosClient.get<MaintenanceTicketDetailsResponse>(
      `${BASE_URL}/${id}`
    );
    return data;
  },

  /**
   * Create a new maintenance ticket
   */
  async createMaintenanceTicket(request: CreateMaintenanceTicketRequest) {
    const { data } = await axiosClient.post<CreateMaintenanceTicketResponse>(
      BASE_URL,
      request
    );
    return data;
  },

  /**
   * Update an existing maintenance ticket
   */
  async updateMaintenanceTicket(id: string, request: UpdateMaintenanceTicketRequest) {
    const { data } = await axiosClient.put<UpdatedMaintenanceTicketResponse>(
      `${BASE_URL}/${id}`,
      request
    );
    return data;
  },

  /**
   * Change the status of a maintenance ticket
   */
  async changeMaintenanceTicketStatus(
    id: string,
    request: ChangeMaintenanceTicketStatusRequest
  ) {
    const { data } = await axiosClient.patch<UpdatedMaintenanceTicketStatusResponse>(
      `${BASE_URL}/${id}/status`,
      request
    );
    return data;
  },

  /**
   * Delete a maintenance ticket
   */
  async deleteMaintenanceTicket(id: string) {
    const { data } = await axiosClient.delete<DeletedMaintenanceTicketResponse>(
      `${BASE_URL}/${id}`
    );
    return data;
  },
};

