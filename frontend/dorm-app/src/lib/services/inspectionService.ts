import type {
    Inspection,
    InspectionStatus,
    RoomInspectionStatus,
    ShortInspection,
} from "@/lib/types/inspection";
import type { PagedResponse } from "@/lib/types/pagination";
import { api } from "../utils/axios-client";

// DTOs
export interface CreateInspectionDto {
  name: string;
  type: string;
  startDate: Date;
  mode: "manual" | "automatic";
  dormitoryId?: string;
  includeSpecialRooms?: boolean;
  rooms?: RoomInfoDto[];
}

export interface RoomInfoDto {
  roomId: string;
  roomNumber?: string;
  floor?: string;
  building?: string;
}

export interface UpdateRoomStatusDto {
  status: RoomInspectionStatus;
  comment?: string;
}

export const inspectionService = {
  async getById(id: string): Promise<Inspection> {
    return api.get<Inspection>(`/inspections/${id}`);
  },

  async list(params?: {
    status?: InspectionStatus;
    type?: string;
    from?: string;
    to?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<PagedResponse<ShortInspection>> {
    return api.get<PagedResponse<ShortInspection>>(`/inspections`, params);
  },

  async create(data: CreateInspectionDto): Promise<string> {
    return api.post<string>(`/inspections`, data);
  },

  async updateRoomStatus(
    inspectionId: string,
    roomId: string,
    data: UpdateRoomStatusDto,
  ): Promise<void> {
    return api.patch<void>(
      `/inspections/${inspectionId}/rooms/${roomId}`,
      data,
    );
  },

  async start(id: string): Promise<void> {
    return api.post<void>(`/inspections/${id}/start`);
  },

  async complete(id: string): Promise<void> {
    return api.post<void>(`/inspections/${id}/complete`);
  },
};
