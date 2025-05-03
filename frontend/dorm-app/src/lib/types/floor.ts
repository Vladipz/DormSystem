export interface CreateFloorRequest {
  buildingId: string;
  number: number;
}

export interface CreateFloorResponse {
  id: string;
}

export interface DeletedFloorResponse {
  id: string;
}

export interface FloorDetailsResponse {
  id: string;
  buildingId: string;
  number: number;
  blocksCount: number;
  buildingName: string;
  buildingAddress: string;
}

export interface FloorsResponse {
  id: string;
  buildingId: string;
  number: number;
  blocksCount: number;
  buildingName: string;
}

export interface UpdateFloorRequest {
  number: number;
}

export interface UpdateFloorResponse {
  id: string;
}
