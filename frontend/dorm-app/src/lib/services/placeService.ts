import { axiosClient } from "@/lib/utils/axios-client";
import {
  PlaceResponse,
  PlaceDetailsResponse,
  CreatePlaceRequest,
  CreatePlaceResponse,
  UpdatePlaceRequest,
  UpdatedPlaceResponse,
  MoveInRequest,
  MoveOutRequest,
  PlaceOccupationResponse,
  GetPlacesParams,
  GetAvailablePlacesParams,
  PlacePagedResponse
} from "@/lib/types/place";

const API_URL = `${import.meta.env.VITE_PLACES_API_URL ?? "http://localhost:5137/api/places"}`;

export class PlaceService {
  /**
   * Get a paginated list of places with optional filters
   */
  static async getPlaces(params?: GetPlacesParams): Promise<PlacePagedResponse> {
    const { data } = await axiosClient.get<PlacePagedResponse>(API_URL, { params });
    return data;
  }

  /**
   * Get a place by ID
   */
  static async getPlaceById(id: string): Promise<PlaceDetailsResponse> {
    const { data } = await axiosClient.get<PlaceDetailsResponse>(`${API_URL}/${id}`);
    return data;
  }

  /**
   * Create a new place
   */
  static async createPlace(request: CreatePlaceRequest): Promise<CreatePlaceResponse> {
    const { data } = await axiosClient.post<CreatePlaceResponse>(API_URL, request);
    return data;
  }

  /**
   * Update an existing place
   */
  static async updatePlace(id: string, request: UpdatePlaceRequest): Promise<UpdatedPlaceResponse> {
    const { data } = await axiosClient.put<UpdatedPlaceResponse>(`${API_URL}/${id}`, request);
    return data;
  }

  /**
   * Move a user into a place
   */
  static async moveIn(id: string, request: MoveInRequest): Promise<PlaceOccupationResponse> {
    const { data } = await axiosClient.post<PlaceOccupationResponse>(
      `${API_URL}/${id}/movein`,
      request
    );
    return data;
  }

  /**
   * Move a user out of a place
   */
  static async moveOut(id: string, request: MoveOutRequest): Promise<PlaceOccupationResponse> {
    const { data } = await axiosClient.post<PlaceOccupationResponse>(
      `${API_URL}/${id}/moveout`,
      request
    );
    return data;
  }

  /**
   * Delete a place
   */
  static async deletePlace(id: string): Promise<void> {
    await axiosClient.delete(`${API_URL}/${id}`);
  }

  /**
   * Get available places
   */
  static async getAvailablePlaces(params?: GetAvailablePlacesParams): Promise<PlacePagedResponse> {
    const { data } = await axiosClient.get<PlacePagedResponse>(`${API_URL}/available`, { params });
    return data;
  }
}
