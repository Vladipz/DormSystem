import { GenderRule } from "./enums";

export interface BlockDetailsResponse {
  id: string;
  floorId: string;
  label: string;
  genderRule: GenderRule;
  floorNumber: string;
  buildingName: string;
}

export interface BlocksResponse {
  id: string;
  floorId: string;
  label: string;
  genderRule: GenderRule;
  roomsCount: number;
}

export interface CreateBlockRequest {
  floorId: string;
  label: string;
  genderRule: string;
}

export interface CreateBlockResponse {
  id: string;
}

export interface DeletedBlockResponse {
  id: string;
}

export interface UpdateBlockRequest {
  label: string;
  genderRule: string;
}

export interface UpdateBlockResponse {
  id: string;
}

export interface BlockInfo {
  id: string;
  label: string;
}
