import { PagedResponse } from "./pagination";

export interface PlaceResponse {
  id: string;
  roomId: string;
  index: number;
  isOccupied?: boolean;
  occupiedByUserId?: string;
  occupiedByUser?: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
  };
  movedInAt?: string;
  movedOutAt?: string;
}

export interface PlaceDetailsResponse extends PlaceResponse {
  room?: {
    id: string;
    number: string;
    blockId: string;
    type: string;
    capacity: number;
    status: string;
  };
}

export interface UserAddressResponse {
  placeId?: string;
  roomId?: string;
  roomLabel?: string;
  floorId?: string;
  floorLabel?: string;
  buildingId?: string;
  buildingName?: string;
  buildingAddress?: string;
  isOccupied: boolean;
  movedInAt?: string;
}

export interface CreatePlaceRequest {
  roomId: string;
  index: number;
}

export interface CreatePlaceResponse {
  id: string;
  roomId: string;
  index: number;
}

export interface UpdatePlaceRequest {
  roomId?: string;
  index?: number;
}

export interface UpdatedPlaceResponse {
  id: string;
  roomId: string;
  index: number;
}

export interface MoveInRequest {
  userId: string;
}

export interface MoveOutRequest {
  movedOutAt?: string;
}

export interface PlaceOccupationResponse {
  id: string;
  movedInAt: string;
  movedOutAt?: string;
}

export interface GetPlacesParams {
  roomId?: string;
  blockId?: string;
  buildingId?: string;
  isOccupied?: boolean;
  page?: number;
  pageSize?: number;
}

export interface GetAvailablePlacesParams {
  buildingId?: string;
  floorId?: string;
  blockId?: string;
  roomId?: string;
  page?: number;
  pageSize?: number;
}

export type PlacePagedResponse = PagedResponse<PlaceResponse>;