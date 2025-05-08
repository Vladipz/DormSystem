export interface BuildingDetailsResponse {
  id: string;
  name: string;
  address: string;
  floorsCount: number;
  yearBuilt: number;
  administratorContact: string;
  isActive: boolean;
}

export interface BuildingsResponse {
  id: string;
  name: string;
  address: string;
  floorsCount: number;
  yearBuilt: number;
  isActive: boolean;
}

export interface CreateBuildingRequest {
  name: string;
  address: string;
  floorsCount: number;
  yearBuilt: number;
  administratorContact: string;
  isActive: boolean;
}

export interface CreateBuildingResponse {
  id: string;
}

export interface DeletedBuildingResponse {
  id: string;
}

export interface UpdateBuildingRequest {
  id: string;
  name: string;
  address: string;
  floorsCount: number;
  yearBuilt: number;
  administratorContact: string;
  isActive: boolean;
}

export interface UpdatedBuildingResponse {
  id: string;
}

export interface BuildingInfo {
  id: string;
  label: string;
}
