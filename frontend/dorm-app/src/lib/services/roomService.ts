// src/api/rooms.ts

import {
  CreateRoomRequest,
  CreateRoomResponse,
  DeletedRoomResponse,
  RoomDetailsResponse,
  RoomsResponse,
  UpdateRoomRequest,
  UpdatedRoomResponse
} from "@/lib/types/room";
import axios from "axios";

const API_URL = `${import.meta.env.VITE_ROOMS_API_URL ?? "http://localhost:5137/api/rooms"}`;

export class RoomService {
  // Отримати список кімнат
  public static async getAllRooms(
    page = 1, 
    pageSize = 100, 
    blockId?: string, 
    buildingId?: string
  ): Promise<RoomsResponse[]> {
    try {
      const res = await axios.get<{ items: RoomsResponse[] }>(`${API_URL}`, {
        params: { page, pageSize, blockId, buildingId }
      });
      return res.data.items;
    } catch (error) {
      console.error("Error fetching rooms:", error);
      throw error;
    }
  }

  // Отримати одну кімнату за ID
  public static async getRoomById(id: string): Promise<RoomDetailsResponse> {
    try {
      const res = await axios.get<RoomDetailsResponse>(`${API_URL}/${id}`);
      return res.data;
    } catch (error) {
      console.error(`Error fetching room ${id}:`, error);
      throw error;
    }
  }

  // Створити нову кімнату
  public static async createRoom(data: CreateRoomRequest): Promise<CreateRoomResponse> {
    try {
      const res = await axios.post<CreateRoomResponse>(`${API_URL}`, data);
      return res.data;
    } catch (error) {
      console.error("Error creating room:", error);
      throw error;
    }
  }

  // Оновити кімнату
  public static async updateRoom(data: UpdateRoomRequest): Promise<UpdatedRoomResponse> {
    try {
      const res = await axios.put<UpdatedRoomResponse>(`${API_URL}/${data.id}`, data);
      return res.data;
    } catch (error) {
      console.error(`Error updating room ${data.id}:`, error);
      throw error;
    }
  }

  // Видалити кімнату
  public static async deleteRoom(id: string): Promise<DeletedRoomResponse> {
    try {
      const res = await axios.delete<DeletedRoomResponse>(`${API_URL}/${id}`);
      return res.data;
    } catch (error) {
      console.error(`Error deleting room ${id}:`, error);
      throw error;
    }
  }
}
