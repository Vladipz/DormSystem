import axios from "axios";
import {
  FloorsResponse,
  FloorDetailsResponse,
  CreateFloorRequest,
  CreateFloorResponse,
  UpdateFloorRequest,
  UpdateFloorResponse,
  DeletedFloorResponse
} from "@/lib/types/floor";

const API_URL = `${import.meta.env.VITE_FLOORS_API_URL ?? "http://localhost:5137/api/floors"}`;

export class FloorService {
  // Отримати список поверхів
  public static async getAllFloors(buildingId?: string, page = 1, pageSize = 100): Promise<FloorsResponse[]> {
    try {
      const res = await axios.get<{ items: FloorsResponse[] }>(`${API_URL}`, {
        params: { page, pageSize, buildingId }
      });
      return res.data.items;
    } catch (error) {
      console.error("Error fetching floors:", error);
      throw error;
    }
  }

  // Отримати поверх за ID
  public static async getFloorById(id: string): Promise<FloorDetailsResponse> {
    try {
      const res = await axios.get<FloorDetailsResponse>(`${API_URL}/${id}`);
      return res.data;
    } catch (error) {
      console.error(`Error fetching floor ${id}:`, error);
      throw error;
    }
  }

  // Створити поверх
  public static async createFloor(data: CreateFloorRequest): Promise<CreateFloorResponse> {
    try {
      const res = await axios.post<CreateFloorResponse>(`${API_URL}`, data);
      return res.data;
    } catch (error) {
      console.error("Error creating floor:", error);
      throw error;
    }
  }

  // Оновити поверх
  public static async updateFloor(id: string, data: UpdateFloorRequest): Promise<UpdateFloorResponse> {
    try {
      const res = await axios.put<UpdateFloorResponse>(`${API_URL}/${id}`, data);
      return res.data;
    } catch (error) {
      console.error(`Error updating floor ${id}:`, error);
      throw error;
    }
  }

  // Видалити поверх
  public static async deleteFloor(id: string): Promise<DeletedFloorResponse> {
    try {
      const res = await axios.delete<DeletedFloorResponse>(`${API_URL}/${id}`);
      return res.data;
    } catch (error) {
      console.error(`Error deleting floor ${id}:`, error);
      throw error;
    }
  }
}
