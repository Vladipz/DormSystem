// src/api/buildings.ts

import {
  BuildingDetailsResponse,
  BuildingsResponse,
  CreateBuildingRequest,
  CreateBuildingResponse,
  DeletedBuildingResponse,
  UpdateBuildingRequest,
  UpdatedBuildingResponse,
} from "@/lib/types/building";
import { axiosClient } from "@/lib/utils/axios-client";

const API_URL = "/buildings";

export class BuildingService {
  // Get paginated list of buildings
  public static async getAllBuildings(
    page = 1,
    pageSize = 100,
    isActive = true
  ): Promise<BuildingsResponse[]> {
    try {
      const res = await axiosClient.get<{ items: BuildingsResponse[] }>(
        `${API_URL}`,
        {
          params: { page, pageSize, isActive },
        }
      );
      return res.data.items;
    } catch (error) {
      console.error("Error fetching buildings:", error);
      throw error;
    }
  }

  // Get full building details by Id
  public static async getBuildingById(
    id: string
  ): Promise<BuildingDetailsResponse> {
    try {
      const res = await axiosClient.get<BuildingDetailsResponse>(`${API_URL}/${id}`);
      return res.data;
    } catch (error) {
      console.error(`Error fetching building ${id}:`, error);
      throw error;
    }
  }

  // Create a new building
  public static async createBuilding(
    data: CreateBuildingRequest
  ): Promise<CreateBuildingResponse> {
    try {
      const res = await axiosClient.post<CreateBuildingResponse>(`${API_URL}`, data);
      return res.data;
    } catch (error) {
      console.error("Error creating building:", error);
      throw error;
    }
  }

  // Update a building
  public static async updateBuilding(
    data: UpdateBuildingRequest
  ): Promise<UpdatedBuildingResponse> {
    try {
      const res = await axiosClient.put<UpdatedBuildingResponse>(
        `${API_URL}/${data.id}`,
        data
      );
      return res.data;
    } catch (error) {
      console.error(`Error updating building ${data.id}:`, error);
      throw error;
    }
  }

  // Delete a building
  public static async deleteBuilding(
    id: string
  ): Promise<DeletedBuildingResponse> {
    try {
      const res = await axiosClient.delete<DeletedBuildingResponse>(
        `${API_URL}/${id}`
      );
      return res.data;
    } catch (error) {
      console.error(`Error deleting building ${id}:`, error);
      throw error;
    }
  }
}
