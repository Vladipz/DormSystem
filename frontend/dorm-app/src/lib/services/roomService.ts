// src/api/rooms.ts

import {
  CreateRoomRequest,
  CreateRoomResponse,
  DeletedRoomPhotoResponse,
  DeletedRoomResponse,
  RoomDetailsResponse,
  RoomsResponse,
  UpdateRoomRequest,
  UpdatedRoomResponse,
  UploadRoomPhotoResponse
} from "@/lib/types/room";
import { api, axiosClient } from "@/lib/utils/axios-client";

const API_BASE = "/rooms";

export class RoomService {
  // Отримати список кімнат
  public static async getAllRooms(
    page = 1,
    pageSize = 100,
    blockId?: string,
    buildingId?: string,
    floorId?: string,
    onlyBlockless?: boolean
  ): Promise<RoomsResponse[]> {
    try {
      const response = await api.get<{ items: RoomsResponse[] }>(API_BASE, {
        page,
        pageSize,
        blockId,
        buildingId,
        floorId,
        onlyBlockless
      });
      return response.items;
    } catch (error) {
      console.error("Error fetching rooms:", error);
      throw error;
    }
  }

  // Отримати одну кімнату за ID
  public static async getRoomById(id: string): Promise<RoomDetailsResponse> {
    try {
      return await api.get<RoomDetailsResponse>(`${API_BASE}/${id}`);
    } catch (error) {
      console.error(`Error fetching room ${id}:`, error);
      throw error;
    }
  }

  // Створити нову кімнату
  public static async createRoom(data: CreateRoomRequest): Promise<CreateRoomResponse> {
    try {
      return await api.post<CreateRoomResponse>(API_BASE, data);
    } catch (error) {
      console.error("Error creating room:", error);
      throw error;
    }
  }

  // Оновити кімнату
  public static async updateRoom(data: UpdateRoomRequest): Promise<UpdatedRoomResponse> {
    try {
      return await api.put<UpdatedRoomResponse>(`${API_BASE}/${data.id}`, data);
    } catch (error) {
      console.error(`Error updating room ${data.id}:`, error);
      throw error;
    }
  }

  // Видалити кімнату
  public static async deleteRoom(id: string): Promise<DeletedRoomResponse> {
    try {
      return await api.delete<DeletedRoomResponse>(`${API_BASE}/${id}`);
    } catch (error) {
      console.error(`Error deleting room ${id}:`, error);
      throw error;
    }
  }

  // Upload room photo
  public static async uploadRoomPhoto(roomId: string, photo: File): Promise<UploadRoomPhotoResponse> {
    try {
      const formData = new FormData();
      formData.append('photo', photo);

      const response = await axiosClient.post<UploadRoomPhotoResponse>(
        `${API_BASE}/${roomId}/photos`, 
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      );
      return response.data;
    } catch (error) {
      console.error(`Error uploading photo for room ${roomId}:`, error);
      throw error;
    }
  }

  // Delete room photo
  public static async deleteRoomPhoto(roomId: string, photoId: string): Promise<DeletedRoomPhotoResponse> {
    try {
      return await api.delete<DeletedRoomPhotoResponse>(`${API_BASE}/${roomId}/photos/${photoId}`);
    } catch (error) {
      console.error(`Error deleting photo ${photoId} for room ${roomId}:`, error);
      throw error;
    }
  }
}
