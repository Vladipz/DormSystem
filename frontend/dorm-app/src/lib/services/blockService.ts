import axios from "axios";
import {
  BlocksResponse,
  BlockDetailsResponse,
  CreateBlockRequest,
  CreateBlockResponse,
  UpdateBlockRequest,
  UpdateBlockResponse,
  DeletedBlockResponse
} from "@/lib/types/block";

const API_URL = `${import.meta.env.VITE_BLOCKS_API_URL ?? "http://localhost:5137/api/blocks"}`;

export class BlockService {
  // Отримати список блоків
  public static async getAllBlocks(floorId?: string, page = 1, pageSize = 100): Promise<BlocksResponse[]> {
    try {
      const res = await axios.get<{ items: BlocksResponse[] }>(`${API_URL}`, {
        params: { page, pageSize, floorId }
      });
      return res.data.items;
    } catch (error) {
      console.error("Error fetching blocks:", error);
      throw error;
    }
  }

  // Отримати блок за ID
  public static async getBlockById(id: string): Promise<BlockDetailsResponse> {
    try {
      const res = await axios.get<BlockDetailsResponse>(`${API_URL}/${id}`);
      return res.data;
    } catch (error) {
      console.error(`Error fetching block ${id}:`, error);
      throw error;
    }
  }

  // Створити новий блок
  public static async createBlock(data: CreateBlockRequest): Promise<CreateBlockResponse> {
    try {
      const res = await axios.post<CreateBlockResponse>(`${API_URL}`, data);
      return res.data;
    } catch (error) {
      console.error("Error creating block:", error);
      throw error;
    }
  }

  // Оновити блок
  public static async updateBlock(id: string, data: UpdateBlockRequest): Promise<UpdateBlockResponse> {
    try {
      const res = await axios.put<UpdateBlockResponse>(`${API_URL}/${id}`, data);
      return res.data;
    } catch (error) {
      console.error(`Error updating block ${id}:`, error);
      throw error;
    }
  }

  // Видалити блок
  public static async deleteBlock(id: string): Promise<DeletedBlockResponse> {
    try {
      const res = await axios.delete<DeletedBlockResponse>(`${API_URL}/${id}`);
      return res.data;
    } catch (error) {
      console.error(`Error deleting block ${id}:`, error);
      throw error;
    }
  }
}
